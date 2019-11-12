namespace ContextLibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MessageHistory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Message",
                c => new
                    {
                        ID = c.Guid(nullable: false, identity: true),
                        ConversationID = c.Guid(nullable: false),
                        PersonID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Conversation", t => t.ConversationID, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.PersonID, cascadeDelete: true)
                .Index(t => t.ConversationID)
                .Index(t => t.PersonID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Message", "PersonID", "dbo.Person");
            DropForeignKey("dbo.Message", "ConversationID", "dbo.Conversation");
            DropIndex("dbo.Message", new[] { "PersonID" });
            DropIndex("dbo.Message", new[] { "ConversationID" });
            DropTable("dbo.Message");
        }
    }
}
