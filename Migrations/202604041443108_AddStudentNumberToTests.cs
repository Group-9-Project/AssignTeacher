namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStudentNumberToTests : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EntranceTests", "StudentNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EntranceTests", "StudentNumber");
        }
    }
}
