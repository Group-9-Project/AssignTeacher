namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAnnouncementsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Announcement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Message = c.String(),
                        DatePosted = c.DateTime(nullable: false),
                        TargetAudience = c.String(),
                        TeacherId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Teacher", t => t.TeacherId, cascadeDelete: true)
                .Index(t => t.TeacherId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Announcement", "TeacherId", "dbo.Teacher");
            DropIndex("dbo.Announcement", new[] { "TeacherId" });
            DropTable("dbo.Announcement");
        }
    }
}
