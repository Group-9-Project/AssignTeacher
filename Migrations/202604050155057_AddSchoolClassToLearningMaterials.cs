namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSchoolClassToLearningMaterials : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LearningMaterial", "SchoolClassId", c => c.Int(nullable: false));
            CreateIndex("dbo.LearningMaterial", "SchoolClassId");
            AddForeignKey("dbo.LearningMaterial", "SchoolClassId", "dbo.SchoolClass", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LearningMaterial", "SchoolClassId", "dbo.SchoolClass");
            DropIndex("dbo.LearningMaterial", new[] { "SchoolClassId" });
            DropColumn("dbo.LearningMaterial", "SchoolClassId");
        }
    }
}
