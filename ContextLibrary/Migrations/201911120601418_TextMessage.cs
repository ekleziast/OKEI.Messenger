namespace ContextLibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TextMessage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Message", "Text", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Message", "Text");
        }
    }
}
