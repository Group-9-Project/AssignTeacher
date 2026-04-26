namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncModelAfterIdentityFix : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Borrowings", "Status", c => c.String(nullable: false, maxLength: 20));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Borrowings", "Status", c => c.Int(nullable: false));
        }
    }
}
