namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sibongakonke : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ReportGeneration", new[] { "StudentAccount_Id" });
            AlterColumn("dbo.ReportGeneration", "StudentAccount_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.ReportGeneration", "StudentAccount_Id");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ReportGeneration", new[] { "StudentAccount_Id" });
            AlterColumn("dbo.ReportGeneration", "StudentAccount_Id", c => c.Int());
            CreateIndex("dbo.ReportGeneration", "StudentAccount_Id");
        }
    }
}
