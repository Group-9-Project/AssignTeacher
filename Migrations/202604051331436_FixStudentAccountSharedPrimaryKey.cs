namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixStudentAccountSharedPrimaryKey : DbMigration
    {
        public override void Up()
        {
            // =====================================================
            // 1. REMOVE LEGACY FK: Borrowings → StudentAccount
            // =====================================================
            DropForeignKey(
                "dbo.Borrowings",
                "StudentId",
                "dbo.StudentAccount"
            );

            DropIndex(
                "dbo.Borrowings",
                new[] { "StudentId" }
            );

            DropColumn(
                "dbo.Borrowings",
                "StudentId"
            );

            // =====================================================
            // 2. REMOVE WRONG FK: AppUsers → StudentAccount
            // =====================================================
            DropForeignKey(
                "dbo.AppUsers",
                "StudentAccount_Id",
                "dbo.StudentAccount"
            );

            DropIndex(
                "dbo.AppUsers",
                new[] { "StudentAccount_Id" }
            );

            DropColumn(
                "dbo.AppUsers",
                "StudentAccount_Id"
            );

            // =====================================================
            // 3. FIX StudentAccount TO SHARED PRIMARY KEY
            // =====================================================
            DropPrimaryKey("dbo.StudentAccount");

            AlterColumn(
                "dbo.StudentAccount",
                "Id",
                c => c.Int(nullable: false)
            );

            AddPrimaryKey(
                "dbo.StudentAccount",
                "Id"
            );

            CreateIndex(
                "dbo.StudentAccount",
                "Id"
            );

            AddForeignKey(
                "dbo.StudentAccount",
                "Id",
                "dbo.AppUsers",
                "Id",
                cascadeDelete: false
            );
        }




        public override void Down()
        {
            DropForeignKey(
                "dbo.StudentAccount",
                "Id",
                "dbo.AppUsers"
            );

            DropIndex(
                "dbo.StudentAccount",
                new[] { "Id" }
            );

            DropPrimaryKey("dbo.StudentAccount");

            AlterColumn(
                "dbo.StudentAccount",
                "Id",
                c => c.Int(nullable: false, identity: true)
            );

            AddPrimaryKey(
                "dbo.StudentAccount",
                "Id"
            );

            AddColumn(
                "dbo.AppUsers",
                "StudentAccount_Id",
                c => c.Int()
            );

            CreateIndex(
                "dbo.AppUsers",
                "StudentAccount_Id"
            );

            AddForeignKey(
                "dbo.AppUsers",
                "StudentAccount_Id",
                "dbo.StudentAccount",
                "Id"
            );

            AddColumn(
                "dbo.Borrowings",
                "StudentId",
                c => c.Int(nullable: false)
            );

            CreateIndex(
                "dbo.Borrowings",
                "StudentId"
            );

            AddForeignKey(
                "dbo.Borrowings",
                "StudentId",
                "dbo.StudentAccount",
                "Id"
            );
        }
    }
}
