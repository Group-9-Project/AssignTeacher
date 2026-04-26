namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeStudentFieldsNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.EntranceTests", "StudentNumber", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.EntranceTests", "StudentNumber", c => c.String(nullable: false));
        }
    }
}
