namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateEntranceTestWithEmailAndStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EntranceTests", "Email", c => c.String());
            AddColumn("dbo.EntranceTests", "IsAllocated", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EntranceTests", "IsAllocated");
            DropColumn("dbo.EntranceTests", "Email");
        }
    }
}
