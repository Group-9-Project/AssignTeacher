namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateApplicationStatusToEnum : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Application", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Application", "Status", c => c.String());
        }
    }
}
