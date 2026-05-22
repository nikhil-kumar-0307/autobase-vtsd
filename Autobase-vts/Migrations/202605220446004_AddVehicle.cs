namespace autobase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVehicle : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Vehicles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VehicleTypeId = c.Int(nullable: false),
                        VehicleName = c.String(nullable: false, maxLength: 100),
                        VehicleType = c.String(nullable: false, maxLength: 50),
                        RegistrationNo = c.String(nullable: false, maxLength: 20),
                        YearOfManufacture = c.Int(nullable: false),
                        Status = c.String(nullable: false, maxLength: 20),
                        Notes = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedBy = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.VehicleTypes", t => t.VehicleTypeId, cascadeDelete: true)
                .Index(t => t.VehicleTypeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Vehicles", "VehicleTypeId", "dbo.VehicleTypes");
            DropIndex("dbo.Vehicles", new[] { "VehicleTypeId" });
            DropTable("dbo.Vehicles");
        }
    }
}
