namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncTeacherAndSubstitutions : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.NotificationLog", "TeacherId", "dbo.Teacher");
            DropIndex("dbo.NotificationLog", new[] { "TeacherId" });
            CreateTable(
                "dbo.LearningMaterial",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        FileName = c.String(),
                        FilePath = c.String(),
                        FileType = c.String(),
                        UploadDate = c.DateTime(nullable: false),
                        TeacherId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Teacher", t => t.TeacherId, cascadeDelete: true)
                .Index(t => t.TeacherId);
            
            AddColumn("dbo.Teacher", "Password", c => c.String());
            AlterColumn("dbo.SchoolClass", "Name", c => c.String());
            AlterColumn("dbo.Subject", "Name", c => c.String());
            AlterColumn("dbo.Subject", "Code", c => c.String());
            AlterColumn("dbo.Teacher", "FirstName", c => c.String());
            AlterColumn("dbo.Teacher", "LastName", c => c.String());
            AlterColumn("dbo.Teacher", "Email", c => c.String());
            AlterColumn("dbo.Substitution", "Status", c => c.String());
            DropColumn("dbo.TimetableSlot", "CreatedAt");
            DropColumn("dbo.Subject", "Description");
            DropColumn("dbo.Teacher", "PasswordLastChanged");
            DropColumn("dbo.NotificationLog", "ErrorMessage");
        }
        
        public override void Down()
        {
            AddColumn("dbo.NotificationLog", "ErrorMessage", c => c.String());
            AddColumn("dbo.Teacher", "PasswordLastChanged", c => c.DateTime());
            AddColumn("dbo.Subject", "Description", c => c.String());
            AddColumn("dbo.TimetableSlot", "CreatedAt", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.LearningMaterial", "TeacherId", "dbo.Teacher");
            DropIndex("dbo.LearningMaterial", new[] { "TeacherId" });
            AlterColumn("dbo.Substitution", "Status", c => c.Int(nullable: false));
            AlterColumn("dbo.Teacher", "Email", c => c.String(nullable: false));
            AlterColumn("dbo.Teacher", "LastName", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Teacher", "FirstName", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Subject", "Code", c => c.String(maxLength: 10));
            AlterColumn("dbo.Subject", "Name", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.SchoolClass", "Name", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.Teacher", "Password");
            DropTable("dbo.LearningMaterial");
            CreateIndex("dbo.NotificationLog", "TeacherId");
            AddForeignKey("dbo.NotificationLog", "TeacherId", "dbo.Teacher", "Id", cascadeDelete: true);
        }
    }
}
