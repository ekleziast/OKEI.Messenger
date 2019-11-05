namespace MessengerWPF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init_migration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "FirstName", c => c.String());
            AddColumn("dbo.Account", "MiddleName", c => c.String());
            AddColumn("dbo.Account", "LastName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Account", "LastName");
            DropColumn("dbo.Account", "MiddleName");
            DropColumn("dbo.Account", "FirstName");
        }
    }
}
