// Data/QmsDepartmentLite.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace autobase.Data
{
    [Table("Departments")]
    public class QmsDepartmentLite
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
    }
}   