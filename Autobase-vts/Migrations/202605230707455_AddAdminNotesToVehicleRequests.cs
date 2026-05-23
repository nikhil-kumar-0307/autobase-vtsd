namespace autobase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAdminNotesToVehicleRequests : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VehicleRequests", "AdminNotes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VehicleRequests", "AdminNotes");
        }
    }
}
