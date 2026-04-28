namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Consultation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Consultation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        status = c.Int(nullable: false),
                        date = c.DateTime(nullable: false),
                        time = c.DateTime(nullable: false),
                        grade = c.String(),
                        subject = c.String(),
                        reason = c.String(),
                        Class_Id = c.Int(),
                        TeacherSubject_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SchoolClass", t => t.Class_Id)
                .ForeignKey("dbo.TeacherSubject", t => t.TeacherSubject_Id)
                .Index(t => t.Class_Id)
                .Index(t => t.TeacherSubject_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Consultation", "TeacherSubject_Id", "dbo.TeacherSubject");
            DropForeignKey("dbo.Consultation", "Class_Id", "dbo.SchoolClass");
            DropIndex("dbo.Consultation", new[] { "TeacherSubject_Id" });
            DropIndex("dbo.Consultation", new[] { "Class_Id" });
            DropTable("dbo.Consultation");
        }
    }
}
