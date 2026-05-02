namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGradeToStudentAccount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StudentAccount", "Grade", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StudentAccount", "Grade");
        }
    }
}
