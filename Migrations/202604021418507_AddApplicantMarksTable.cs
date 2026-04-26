namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddApplicantMarksTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApplicantMark",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SubjectName = c.String(nullable: false),
                        Percentage = c.Int(nullable: false),
                        ApplicationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Application", t => t.ApplicationId, cascadeDelete: true)
                .Index(t => t.ApplicationId);
            
            AddColumn("dbo.Application", "AverageMark", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApplicantMark", "ApplicationId", "dbo.Application");
            DropIndex("dbo.ApplicantMark", new[] { "ApplicationId" });
            DropColumn("dbo.Application", "AverageMark");
            DropTable("dbo.ApplicantMark");
        }
    }
}
