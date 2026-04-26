namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SchoolClass",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Grade = c.String(),
                        StudentCount = c.Int(),
                        Room = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TimetableSlot",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeacherId = c.Int(nullable: false),
                        SubjectId = c.Int(nullable: false),
                        ClassId = c.Int(nullable: false),
                        DayOfWeek = c.Int(nullable: false),
                        Period = c.Int(nullable: false),
                        Room = c.String(),
                        Notes = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SchoolClass", t => t.ClassId)
                .ForeignKey("dbo.Subject", t => t.SubjectId)
                .ForeignKey("dbo.Teacher", t => t.TeacherId)
                .Index(t => t.TeacherId)
                .Index(t => t.SubjectId)
                .Index(t => t.ClassId);
            
            CreateTable(
                "dbo.Subject",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Code = c.String(maxLength: 10),
                        Description = c.String(),
                        Color = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TeacherSubject",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeacherId = c.Int(nullable: false),
                        SubjectId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subject", t => t.SubjectId, cascadeDelete: true)
                .ForeignKey("dbo.Teacher", t => t.TeacherId, cascadeDelete: true)
                .Index(t => t.TeacherId)
                .Index(t => t.SubjectId);
            
            CreateTable(
                "dbo.Teacher",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false),
                        Phone = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Substitution",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TimetableSlotId = c.Int(nullable: false),
                        OriginalTeacherId = c.Int(nullable: false),
                        SubstituteTeacherId = c.Int(nullable: false),
                        SubstitutionDate = c.DateTime(nullable: false),
                        Reason = c.String(),
                        Status = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Teacher", t => t.OriginalTeacherId)
                .ForeignKey("dbo.Teacher", t => t.SubstituteTeacherId)
                .ForeignKey("dbo.TimetableSlot", t => t.TimetableSlotId)
                .Index(t => t.TimetableSlotId)
                .Index(t => t.OriginalTeacherId)
                .Index(t => t.SubstituteTeacherId);
            
            CreateTable(
                "dbo.NotificationLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeacherId = c.Int(nullable: false),
                        Subject = c.String(),
                        Body = c.String(),
                        Sent = c.Boolean(nullable: false),
                        ErrorMessage = c.String(),
                        SentAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Teacher", t => t.TeacherId, cascadeDelete: true)
                .Index(t => t.TeacherId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NotificationLog", "TeacherId", "dbo.Teacher");
            DropForeignKey("dbo.TimetableSlot", "TeacherId", "dbo.Teacher");
            DropForeignKey("dbo.TimetableSlot", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.TeacherSubject", "TeacherId", "dbo.Teacher");
            DropForeignKey("dbo.Substitution", "TimetableSlotId", "dbo.TimetableSlot");
            DropForeignKey("dbo.Substitution", "SubstituteTeacherId", "dbo.Teacher");
            DropForeignKey("dbo.Substitution", "OriginalTeacherId", "dbo.Teacher");
            DropForeignKey("dbo.TeacherSubject", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.TimetableSlot", "ClassId", "dbo.SchoolClass");
            DropIndex("dbo.NotificationLog", new[] { "TeacherId" });
            DropIndex("dbo.Substitution", new[] { "SubstituteTeacherId" });
            DropIndex("dbo.Substitution", new[] { "OriginalTeacherId" });
            DropIndex("dbo.Substitution", new[] { "TimetableSlotId" });
            DropIndex("dbo.TeacherSubject", new[] { "SubjectId" });
            DropIndex("dbo.TeacherSubject", new[] { "TeacherId" });
            DropIndex("dbo.TimetableSlot", new[] { "ClassId" });
            DropIndex("dbo.TimetableSlot", new[] { "SubjectId" });
            DropIndex("dbo.TimetableSlot", new[] { "TeacherId" });
            DropTable("dbo.NotificationLog");
            DropTable("dbo.Substitution");
            DropTable("dbo.Teacher");
            DropTable("dbo.TeacherSubject");
            DropTable("dbo.Subject");
            DropTable("dbo.TimetableSlot");
            DropTable("dbo.SchoolClass");
        }
    }
}
