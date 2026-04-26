namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGradeRemoveReason : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Application", "GradeApplyingFor", c => c.String(nullable: false));
            DropColumn("dbo.Application", "Reason");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Application", "Reason", c => c.String(nullable: false));
            DropColumn("dbo.Application", "GradeApplyingFor");
        }
    }
}
