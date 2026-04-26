namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddParentIdToApplication : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Application", "ParentId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Application", "ParentId");
        }
    }
}
