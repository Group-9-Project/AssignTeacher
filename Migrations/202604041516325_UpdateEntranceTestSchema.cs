namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateEntranceTestSchema : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EntranceTests", "Date", c => c.DateTime(nullable: false));
            AlterColumn("dbo.EntranceTests", "StudentNumber", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.EntranceTests", "StudentNumber", c => c.String());
            DropColumn("dbo.EntranceTests", "Date");
        }
    }
}
