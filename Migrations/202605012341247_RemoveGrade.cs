namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveGrade : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ReportGeneration", "Grade");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ReportGeneration", "Grade", c => c.String(nullable: false));
        }
    }
}
