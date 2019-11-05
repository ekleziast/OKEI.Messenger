namespace MessengerWPF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init_migration_2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChatHistory",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ChatID = c.Guid(nullable: false),
                        DateTime = c.DateTime(nullable: false),
                        Message = c.Binary(),
                        IsFile = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Chat", t => t.ChatID, cascadeDelete: true)
                .Index(t => t.ChatID);
            
            CreateTable(
                "dbo.Chat",
                c => new
                    {
                        ID = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.ChatMembers",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ChatID = c.Guid(nullable: false),
                        AccountID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Account", t => t.AccountID, cascadeDelete: true)
                .ForeignKey("dbo.Chat", t => t.ChatID, cascadeDelete: true)
                .Index(t => t.ChatID)
                .Index(t => t.AccountID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatHistory", "ChatID", "dbo.Chat");
            DropForeignKey("dbo.ChatMembers", "ChatID", "dbo.Chat");
            DropForeignKey("dbo.ChatMembers", "AccountID", "dbo.Account");
            DropIndex("dbo.ChatMembers", new[] { "AccountID" });
            DropIndex("dbo.ChatMembers", new[] { "ChatID" });
            DropIndex("dbo.ChatHistory", new[] { "ChatID" });
            DropTable("dbo.ChatMembers");
            DropTable("dbo.Chat");
            DropTable("dbo.ChatHistory");
        }
    }
}
