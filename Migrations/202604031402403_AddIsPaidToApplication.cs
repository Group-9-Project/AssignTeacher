namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsPaidToApplication : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Application", "IsPaid", c => c.Boolean(nullable: false));
            AddColumn("dbo.Application", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Application", "Status");
            DropColumn("dbo.Application", "IsPaid");
        }
    }
}
