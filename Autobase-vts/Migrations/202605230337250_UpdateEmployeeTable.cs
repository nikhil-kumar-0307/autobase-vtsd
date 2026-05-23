namespace autobase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateEmployeeTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.VehicleRequests", "EmployeeId", "dbo.Employees");
            DropForeignKey("dbo.VehicleRequests", "VehicleId", "dbo.Vehicles");
            DropIndex("dbo.VehicleRequests", new[] { "EmployeeId" });
            DropIndex("dbo.VehicleRequests", new[] { "VehicleId" });
            DropTable("dbo.VehicleRequests");
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.VehicleRequests", "VehicleId");
            CreateIndex("dbo.VehicleRequests", "EmployeeId");
            AddForeignKey("dbo.VehicleRequests", "VehicleId", "dbo.Vehicles", "Id");
            AddForeignKey("dbo.VehicleRequests", "EmployeeId", "dbo.Employees", "Id");
        }
    }
}
