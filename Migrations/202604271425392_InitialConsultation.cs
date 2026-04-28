namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialConsultation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Consultation", "Class_Id", "dbo.SchoolClass");
            DropForeignKey("dbo.Consultation", "TeacherSubject_Id", "dbo.TeacherSubject");
            DropIndex("dbo.Consultation", new[] { "Class_Id" });
            DropIndex("dbo.Consultation", new[] { "TeacherSubject_Id" });
            RenameColumn(table: "dbo.Consultation", name: "Class_Id", newName: "ClassId");
            RenameColumn(table: "dbo.Consultation", name: "TeacherSubject_Id", newName: "TeacherSubjectId");
            AlterColumn("dbo.Consultation", "ClassId", c => c.Int(nullable: false));
            AlterColumn("dbo.Consultation", "TeacherSubjectId", c => c.Int(nullable: false));
            CreateIndex("dbo.Consultation", "TeacherSubjectId");
            CreateIndex("dbo.Consultation", "ClassId");
            AddForeignKey("dbo.Consultation", "ClassId", "dbo.SchoolClass", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Consultation", "TeacherSubjectId", "dbo.TeacherSubject", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Consultation", "TeacherSubjectId", "dbo.TeacherSubject");
            DropForeignKey("dbo.Consultation", "ClassId", "dbo.SchoolClass");
            DropIndex("dbo.Consultation", new[] { "ClassId" });
            DropIndex("dbo.Consultation", new[] { "TeacherSubjectId" });
            AlterColumn("dbo.Consultation", "TeacherSubjectId", c => c.Int());
            AlterColumn("dbo.Consultation", "ClassId", c => c.Int());
            RenameColumn(table: "dbo.Consultation", name: "TeacherSubjectId", newName: "TeacherSubject_Id");
            RenameColumn(table: "dbo.Consultation", name: "ClassId", newName: "Class_Id");
            CreateIndex("dbo.Consultation", "TeacherSubject_Id");
            CreateIndex("dbo.Consultation", "Class_Id");
            AddForeignKey("dbo.Consultation", "TeacherSubject_Id", "dbo.TeacherSubject", "Id");
            AddForeignKey("dbo.Consultation", "Class_Id", "dbo.SchoolClass", "Id");
        }
    }
}
