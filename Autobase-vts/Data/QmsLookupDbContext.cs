using System.Data.Entity;

namespace autobase.Data
{
    // Separate context because it points at the QMS database, not Autobase's.
    public class QmsLookupDbContext : DbContext
    {
        public QmsLookupDbContext() : base("QmsLookupConnection") { }

        public DbSet<QmsEmployeeMasterLite> EmployeeMasters { get; set; }
    }
}