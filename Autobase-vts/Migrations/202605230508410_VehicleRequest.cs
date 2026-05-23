namespace autobase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VehicleRequest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VehicleRequests",
                c => new
                    {
                        RequestId = c.Int(nullable: false, identity: true),
                        VehicleId = c.Int(nullable: false),
                        VehicleName = c.String(),
                        RegistrationNo = c.String(),
                        EmployeeNumber = c.String(),
                        Purpose = c.String(nullable: false),
                        RequiredFrom = c.DateTime(nullable: false),
                        RequiredUntil = c.DateTime(nullable: false),
                        Destination = c.String(nullable: false),
                        AdditionalNotes = c.String(),
                        Status = c.String(),
                        RequestedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.RequestId);
            
            AddColumn("dbo.Vehicles", "FuelType", c => c.String(maxLength: 20));
            AddColumn("dbo.Vehicles", "SeatingCapacity", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Vehicles", "SeatingCapacity");
            DropColumn("dbo.Vehicles", "FuelType");
            DropTable("dbo.VehicleRequests");
        }
    }
}
