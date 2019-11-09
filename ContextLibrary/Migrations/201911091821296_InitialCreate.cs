namespace ContextLibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Conversation",
                c => new
                    {
                        ID = c.Guid(nullable: false, identity: true),
                        Title = c.String(),
                        PhotoSource = c.Binary(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Member",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PersonID = c.Guid(nullable: false),
                        ConversationID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Conversation", t => t.ConversationID, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.PersonID, cascadeDelete: true)
                .Index(t => t.PersonID)
                .Index(t => t.ConversationID);
            
            CreateTable(
                "dbo.Person",
                c => new
                    {
                        ID = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                        SurName = c.String(),
                        Login = c.String(),
                        Password = c.String(),
                        StatusID = c.Int(),
                        PhotoID = c.Guid(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Status", t => t.StatusID)
                .Index(t => t.StatusID);
            
            CreateTable(
                "dbo.Photo",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        PhotoSource = c.Binary(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Person", t => t.ID)
                .Index(t => t.ID);
            
            CreateTable(
                "dbo.Status",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Member", "PersonID", "dbo.Person");
            DropForeignKey("dbo.Person", "StatusID", "dbo.Status");
            DropForeignKey("dbo.Photo", "ID", "dbo.Person");
            DropForeignKey("dbo.Member", "ConversationID", "dbo.Conversation");
            DropIndex("dbo.Photo", new[] { "ID" });
            DropIndex("dbo.Person", new[] { "StatusID" });
            DropIndex("dbo.Member", new[] { "ConversationID" });
            DropIndex("dbo.Member", new[] { "PersonID" });
            DropTable("dbo.Status");
            DropTable("dbo.Photo");
            DropTable("dbo.Person");
            DropTable("dbo.Member");
            DropTable("dbo.Conversation");
        }
    }
}
