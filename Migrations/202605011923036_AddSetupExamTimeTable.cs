namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSetupExamTimeTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Setup_exam_time",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Exam_name = c.String(nullable: false),
                    venue = c.String(nullable: false),
                    Exam_Starttime = c.String(nullable: false),
                    Exam_Endtime = c.String(nullable: false),
                    Exam_date = c.DateTime(nullable: false),
                    Grade = c.String(),
                })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropTable("dbo.Setup_exam_time");
        }
    }
}
