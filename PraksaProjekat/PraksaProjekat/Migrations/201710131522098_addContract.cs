namespace PraksaProjekat.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addContract : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Contracts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExpiryDate = c.DateTime(nullable: false),
                        TipUgovora = c.String(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Contracts", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Contracts", new[] { "User_Id" });
            DropTable("dbo.Contracts");
        }
    }
}
