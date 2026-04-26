namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddClassToAnnouncements : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Announcement", "SchoolClassId", c => c.Int());
            CreateIndex("dbo.Announcement", "SchoolClassId");
            AddForeignKey("dbo.Announcement", "SchoolClassId", "dbo.SchoolClass", "Id");
            DropColumn("dbo.Announcement", "TargetAudience");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Announcement", "TargetAudience", c => c.String());
            DropForeignKey("dbo.Announcement", "SchoolClassId", "dbo.SchoolClass");
            DropIndex("dbo.Announcement", new[] { "SchoolClassId" });
            DropColumn("dbo.Announcement", "SchoolClassId");
        }
    }
}
