namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class IntegratedSystemMigration : DbMigration
    {
        public override void Up()
        {
            
        }

        public override void Down()
        {
            DropForeignKey("dbo.ClassAttendance", "StudentId", "dbo.Student");
            DropForeignKey("dbo.ClassAttendance", "ClassId", "dbo.OnlineClass");
            DropForeignKey("dbo.OnlineClass", "TeacherId", "dbo.Teacher");
            DropIndex("dbo.ClassAttendance", new[] { "StudentId" });
            DropIndex("dbo.ClassAttendance", new[] { "ClassId" });
            DropIndex("dbo.OnlineClass", new[] { "TeacherId" });
            DropTable("dbo.ClassAttendance");
            DropTable("dbo.OnlineClass");
        }
    }
}