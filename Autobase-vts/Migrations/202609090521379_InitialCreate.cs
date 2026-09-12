namespace autobase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Employees",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EmployeeNumber = c.String(nullable: false, maxLength: 20),
                        FullName = c.String(nullable: false, maxLength: 100),
                        Email = c.String(maxLength: 150),
                        PasswordHash = c.String(nullable: false),
                        Role = c.String(nullable: false, maxLength: 20),
                        MobileNumber = c.String(),
                        Designation = c.String(),
                        Department = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        LastLoginAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
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
                        Status = c.String(),
                        AdminNotes = c.String(),
                        RequestedOn = c.DateTime(nullable: false),
                        HodNotes = c.String(),
                        HodApprovedBy = c.String(),
                        HodApprovedOn = c.DateTime(),
                        FinalApprovedBy = c.String(),
                        FinalApprovedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.RequestId);
            
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
                        FuelType = c.String(maxLength: 20),
                        SeatingCapacity = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedBy = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.VehicleTypes", t => t.VehicleTypeId, cascadeDelete: true)
                .Index(t => t.VehicleTypeId);
            
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
            DropForeignKey("dbo.Vehicles", "VehicleTypeId", "dbo.VehicleTypes");
            DropIndex("dbo.Vehicles", new[] { "VehicleTypeId" });
            DropTable("dbo.VehicleTypes");
            DropTable("dbo.Vehicles");
            DropTable("dbo.VehicleRequests");
            DropTable("dbo.Employees");
        }
    }
}
