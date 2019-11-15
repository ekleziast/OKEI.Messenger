namespace ContextLibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DateTime_Message : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Message", "DateTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Message", "DateTime");
        }
    }
}
