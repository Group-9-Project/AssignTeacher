namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLibraryAndAppUser : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AppUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 150),
                        PasswordHash = c.String(nullable: false),
                        Role = c.Int(nullable: false),
                        Grade = c.Int(),
                        IsBlocked = c.Boolean(nullable: false),
                        BlockReason = c.String(maxLength: 400),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Borrowings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AppUserId = c.Int(nullable: false),
                        BookId = c.Int(nullable: false),
                        BorrowedDate = c.DateTime(nullable: false),
                        DueDate = c.DateTime(nullable: false),
                        ReturnedDate = c.DateTime(),
                        Status = c.Int(nullable: false),
                        Notes = c.String(maxLength: 400),
                        FineAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FinePaid = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Books", t => t.BookId)
                .ForeignKey("dbo.AppUsers", t => t.AppUserId)
                .Index(t => t.AppUserId)
                .Index(t => t.BookId);
            
            CreateTable(
                "dbo.Books",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Author = c.String(nullable: false, maxLength: 150),
                        ISBN = c.String(maxLength: 20),
                        Genre = c.String(maxLength: 80),
                        Publisher = c.String(maxLength: 120),
                        PublicationYear = c.Int(),
                        TotalCopies = c.Int(nullable: false),
                        AvailableCopies = c.Int(nullable: false),
                        Description = c.String(maxLength: 600),
                        MinGrade = c.Int(),
                        MaxGrade = c.Int(),
                        AddedAt = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Borrowings", "AppUserId", "dbo.AppUsers");
            DropForeignKey("dbo.Borrowings", "BookId", "dbo.Books");
            DropIndex("dbo.Borrowings", new[] { "BookId" });
            DropIndex("dbo.Borrowings", new[] { "AppUserId" });
            DropTable("dbo.Books");
            DropTable("dbo.Borrowings");
            DropTable("dbo.AppUsers");
        }
    }
}
