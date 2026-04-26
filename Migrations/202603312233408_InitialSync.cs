namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialSync : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Teacher", "TemporaryPassword", c => c.String());
            AddColumn("dbo.Teacher", "PasswordLastChanged", c => c.DateTime());
            AddColumn("dbo.NotificationLog", "Timestamp", c => c.DateTime(nullable: false));
            DropColumn("dbo.NotificationLog", "SentAt");
        }
        
        public override void Down()
        {
            AddColumn("dbo.NotificationLog", "SentAt", c => c.DateTime(nullable: false));
            DropColumn("dbo.NotificationLog", "Timestamp");
            DropColumn("dbo.Teacher", "PasswordLastChanged");
            DropColumn("dbo.Teacher", "TemporaryPassword");
        }
    }
}
