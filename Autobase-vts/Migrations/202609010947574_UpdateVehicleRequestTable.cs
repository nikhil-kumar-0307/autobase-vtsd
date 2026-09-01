namespace autobase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateVehicleRequestTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VehicleRequests", "HodNotes", c => c.String());
            AddColumn("dbo.VehicleRequests", "HodApprovedBy", c => c.String());
            AddColumn("dbo.VehicleRequests", "HodApprovedOn", c => c.DateTime());
            AddColumn("dbo.VehicleRequests", "FinalApprovedBy", c => c.String());
            AddColumn("dbo.VehicleRequests", "FinalApprovedOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VehicleRequests", "FinalApprovedOn");
            DropColumn("dbo.VehicleRequests", "FinalApprovedBy");
            DropColumn("dbo.VehicleRequests", "HodApprovedOn");
            DropColumn("dbo.VehicleRequests", "HodApprovedBy");
            DropColumn("dbo.VehicleRequests", "HodNotes");
        }
    }
}
