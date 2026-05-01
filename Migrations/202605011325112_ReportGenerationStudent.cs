namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReportGenerationStudent : DbMigration
    {
        public override void Up()
        {
            // Guarded rename: only rename if target doesn't already exist and source exists
            Sql(@"
IF OBJECT_ID(N'dbo.StudentAccount','U') IS NULL AND OBJECT_ID(N'dbo.StudentAccounts','U') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.StudentAccounts', N'StudentAccount';
END
");

            // Existing migration operations (kept as-is)
            DropForeignKey("dbo.ReportGeneration", "ClassId", "dbo.SchoolClass");
            DropForeignKey("dbo.ReportGeneration", "SubjectId", "dbo.TeacherSubject");
            DropIndex("dbo.ReportGeneration", new[] { "SubjectId" });
            DropIndex("dbo.ReportGeneration", new[] { "ClassId" });
            AddColumn("dbo.ReportGeneration", "TeacherId", c => c.Int(nullable: false));
            AddColumn("dbo.ReportGeneration", "Subject", c => c.Int(nullable: false));
            AddColumn("dbo.ReportGeneration", "StudentAccount_Id", c => c.Int());
            AlterColumn("dbo.ReportGeneration", "StudentName", c => c.String());
            AlterColumn("dbo.ReportGeneration", "Grade", c => c.String(nullable: false));
            CreateIndex("dbo.ReportGeneration", "TeacherId");
            CreateIndex("dbo.ReportGeneration", "StudentAccount_Id");
            AddForeignKey("dbo.ReportGeneration", "StudentAccount_Id", "dbo.StudentAccount", "Id");
            AddForeignKey("dbo.ReportGeneration", "TeacherId", "dbo.Teacher", "Id", cascadeDelete: true);
            DropColumn("dbo.ReportGeneration", "SubjectId");
            DropColumn("dbo.ReportGeneration", "ClassId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ReportGeneration", "ClassId", c => c.Int(nullable: false));
            AddColumn("dbo.ReportGeneration", "SubjectId", c => c.Int(nullable: false));
            DropForeignKey("dbo.ReportGeneration", "TeacherId", "dbo.Teacher");
            DropForeignKey("dbo.ReportGeneration", "StudentAccount_Id", "dbo.StudentAccount");
            DropIndex("dbo.ReportGeneration", new[] { "StudentAccount_Id" });
            DropIndex("dbo.ReportGeneration", new[] { "TeacherId" });
            AlterColumn("dbo.ReportGeneration", "Grade", c => c.String());
            AlterColumn("dbo.ReportGeneration", "StudentName", c => c.String(nullable: false));
            DropColumn("dbo.ReportGeneration", "StudentAccount_Id");
            DropColumn("dbo.ReportGeneration", "Subject");
            DropColumn("dbo.ReportGeneration", "TeacherId");
            CreateIndex("dbo.ReportGeneration", "ClassId");
            CreateIndex("dbo.ReportGeneration", "SubjectId");
            AddForeignKey("dbo.ReportGeneration", "SubjectId", "dbo.TeacherSubject", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ReportGeneration", "ClassId", "dbo.SchoolClass", "Id", cascadeDelete: true);
            RenameTable(name: "dbo.StudentAccount", newName: "StudentAccounts");
        }
    }
}
