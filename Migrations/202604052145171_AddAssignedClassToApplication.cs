namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAssignedClassToApplication : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Application", "AssignedClassId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Application", "AssignedClassId");
        }
    }
}
