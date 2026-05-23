namespace autobase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVehicleRequests : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VehicleRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EmployeeId = c.Int(nullable: false),
                        VehicleId = c.Int(nullable: false),
                        Status = c.String(nullable: false, maxLength: 20),
                        Reason = c.String(maxLength: 300),
                        AdminRemarks = c.String(maxLength: 300),
                        RequestedAt = c.DateTime(nullable: false),
                        ReviewedAt = c.DateTime(),
                        ReviewedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Employees", t => t.EmployeeId)
                .ForeignKey("dbo.Vehicles", t => t.VehicleId)
                .Index(t => t.EmployeeId)
                .Index(t => t.VehicleId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.VehicleRequests", "VehicleId", "dbo.Vehicles");
            DropForeignKey("dbo.VehicleRequests", "EmployeeId", "dbo.Employees");
            DropIndex("dbo.VehicleRequests", new[] { "VehicleId" });
            DropIndex("dbo.VehicleRequests", new[] { "EmployeeId" });
            DropTable("dbo.VehicleRequests");
        }
    }
}
