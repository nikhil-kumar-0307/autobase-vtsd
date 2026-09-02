using System.ComponentModel.DataAnnotations.Schema;

namespace autobase.Data
{
    // Read-only mirror of QMS's EmployeeMaster table — used only to authenticate
    // employees who exist in QMS but were never explicitly set up in Autobase's
    // own Employees table.
    [Table("EmployeeMaster")]
    public class QmsEmployeeMasterLite
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Password { get; set; }
    }
}