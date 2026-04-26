namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FinalIntegration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Borrowings", "StudentId", c => c.Int(nullable: false));
            AddColumn("dbo.StudentAccount", "IsBlocked", c => c.Boolean(nullable: false));
            AddColumn("dbo.StudentAccount", "BlockReason", c => c.String(maxLength: 400));
            AddColumn("dbo.Student", "IsBlocked", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Borrowings", "StudentId");
            AddForeignKey("dbo.Borrowings", "StudentId", "dbo.StudentAccount", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Borrowings", "StudentId", "dbo.StudentAccount");
            DropIndex("dbo.Borrowings", new[] { "StudentId" });
            DropColumn("dbo.Student", "IsBlocked");
            DropColumn("dbo.StudentAccount", "BlockReason");
            DropColumn("dbo.StudentAccount", "IsBlocked");
            DropColumn("dbo.Borrowings", "StudentId");
        }
    }
}
