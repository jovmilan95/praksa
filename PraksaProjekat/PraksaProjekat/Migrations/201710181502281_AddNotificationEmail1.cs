namespace PraksaProjekat.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotificationEmail1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NotificationEmails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.NotificationEmails");
        }
    }
}
