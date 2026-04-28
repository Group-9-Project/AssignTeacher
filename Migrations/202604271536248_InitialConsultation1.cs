namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialConsultation1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Consultation", "ParentId", c => c.Int());
            CreateIndex("dbo.Consultation", "ParentId");
            AddForeignKey("dbo.Consultation", "ParentId", "dbo.Parent", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Consultation", "ParentId", "dbo.Parent");
            DropIndex("dbo.Consultation", new[] { "ParentId" });
            DropColumn("dbo.Consultation", "ParentId");
        }
    }
}
