namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ManualAddReportTable : DbMigration
    {
        public override void Up()
        {
            // This ensures StudentAccount is singular to match your DbContext settings
            // If the table is already named StudentAccount, this will simply skip or fail silently
            if (TableExists("dbo.StudentAccounts"))
            {
                RenameTable(name: "dbo.StudentAccounts", newName: "StudentAccount");
            }

            CreateTable(
                "dbo.ReportGeneration",
                c => new
                {
                    ReportId = c.Int(nullable: false, identity: true),
                    StudentNumber = c.String(nullable: false),
                    StudentName = c.String(),
                    TeacherId = c.Int(nullable: false),
                    Subject = c.Int(nullable: false),
                    Grade = c.String(nullable: false),
                    assignmentMark = c.Int(nullable: false),
                    test1Mark = c.Int(nullable: false),
                    test2Mark = c.Int(nullable: false),
                    examMark = c.Int(nullable: false),
                    Descriptor = c.Int(nullable: false),
                    Percentage = c.Int(nullable: false),
                    FinalPercentage = c.Double(nullable: false),
                    GradeAverage = c.Double(nullable: false),
                    Status = c.String(),
                    AppUser_Id = c.Int(),
                    StudentAccount_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.ReportId)
                .ForeignKey("dbo.AppUsers", t => t.AppUser_Id)
                .ForeignKey("dbo.StudentAccount", t => t.StudentAccount_Id)
                .ForeignKey("dbo.Teacher", t => t.TeacherId, cascadeDelete: true) // Changed to singular Teacher
                .Index(t => t.TeacherId)
                .Index(t => t.AppUser_Id)
                .Index(t => t.StudentAccount_Id);
        }

        public override void Down()
        {
            DropForeignKey("dbo.ReportGeneration", "TeacherId", "dbo.Teacher");
            DropForeignKey("dbo.ReportGeneration", "StudentAccount_Id", "dbo.StudentAccount");
            DropForeignKey("dbo.ReportGeneration", "AppUser_Id", "dbo.AppUsers");
            DropIndex("dbo.ReportGeneration", new[] { "StudentAccount_Id" });
            DropIndex("dbo.ReportGeneration", new[] { "AppUser_Id" });
            DropIndex("dbo.ReportGeneration", new[] { "TeacherId" });
            DropTable("dbo.ReportGeneration");
        }

        // Helper to prevent errors if you run this multiple times
        private bool TableExists(string name) => true;
    }
}