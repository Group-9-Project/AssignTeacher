namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStudentNamesToApplication : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Application", "FirstName", c => c.String(nullable: false));
            AddColumn("dbo.Application", "LastName", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Application", "LastName");
            DropColumn("dbo.Application", "FirstName");
        }
    }
}
