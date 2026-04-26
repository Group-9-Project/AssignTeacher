namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateStudentModelAndAdmissions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Student",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentNumber = c.String(nullable: false),
                        Password = c.String(nullable: false),
                        FullName = c.String(nullable: false),
                        ParentEmail = c.String(nullable: false),
                        Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RegistrationFeePaid = c.Boolean(nullable: false),
                        ApplicationId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Application", t => t.ApplicationId, cascadeDelete: true)
                .Index(t => t.ApplicationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Student", "ApplicationId", "dbo.Application");
            DropIndex("dbo.Student", new[] { "ApplicationId" });
            DropTable("dbo.Student");
        }
    }
}
