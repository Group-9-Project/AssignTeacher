namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStudentNumberToAppUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AppUsers", "StudentNumber", c => c.String(maxLength: 20));
            AddColumn("dbo.AppUsers", "StudentAccount_Id", c => c.Int());
            CreateIndex("dbo.AppUsers", "StudentAccount_Id");
            AddForeignKey("dbo.AppUsers", "StudentAccount_Id", "dbo.StudentAccount", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AppUsers", "StudentAccount_Id", "dbo.StudentAccount");
            DropIndex("dbo.AppUsers", new[] { "StudentAccount_Id" });
            DropColumn("dbo.AppUsers", "StudentAccount_Id");
            DropColumn("dbo.AppUsers", "StudentNumber");
        }
    }
}
