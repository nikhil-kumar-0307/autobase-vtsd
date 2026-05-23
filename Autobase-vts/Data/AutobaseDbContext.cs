using System.Data.Entity;
using autobase.Models;



namespace autobase.Data
{
    public class AutobaseDbContext : DbContext
    {
        public AutobaseDbContext() : base("AutobaseConnection")
        {
        }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<VehicleTypes> VehicleTypes { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleRequest> VehicleRequests { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}