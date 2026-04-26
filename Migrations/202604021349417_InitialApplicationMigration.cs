namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialApplicationMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Application",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateOfBirth = c.DateTime(nullable: false),
                        Gender = c.String(nullable: false),
                        HomeAddress = c.String(nullable: false),
                        City = c.String(nullable: false),
                        State = c.String(nullable: false),
                        PostalCode = c.String(nullable: false),
                        Allergies = c.String(),
                        SpecialNeeds = c.String(),
                        PreviousSchoolName = c.String(nullable: false),
                        Reason = c.String(nullable: false),
                        ParentName = c.String(nullable: false),
                        Relationship = c.String(nullable: false),
                        PhoneNumber = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        StudentCertificatePath = c.String(),
                        TransferDocumentPath = c.String(),
                        ParentIdPath = c.String(),
                        SubmissionDate = c.DateTime(nullable: false),
                        IsProcessed = c.Boolean(nullable: false),
                        AgreedToCodeOfConduct = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Parent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        Password = c.String(nullable: false),
                        ResetCode = c.String(),
                        ResetExpiry = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Teacher", "IsAdmin", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Teacher", "IsAdmin");
            DropTable("dbo.Parent");
            DropTable("dbo.Application");
        }
    }
}
