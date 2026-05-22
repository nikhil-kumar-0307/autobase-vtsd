namespace autobase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVehicleTypes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VehicleTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VehicleName = c.String(nullable: false, maxLength: 100),
                        VehicleType = c.String(nullable: false, maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedBy = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.VehicleTypes");
        }
    }
}
