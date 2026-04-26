namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StudentAccount : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StudentAccount",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentNumber = c.String(nullable: false),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        Email = c.String(),
                        TemporaryPassword = c.String(),
                        Password = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ApplicationId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.StudentAccount");
        }
    }
}
