namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveFilePathsFromApplication : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Application", "StudentCertificatePath");
            DropColumn("dbo.Application", "TransferDocumentPath");
            DropColumn("dbo.Application", "ParentIdPath");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Application", "ParentIdPath", c => c.String());
            AddColumn("dbo.Application", "TransferDocumentPath", c => c.String());
            AddColumn("dbo.Application", "StudentCertificatePath", c => c.String());
        }
    }
}
