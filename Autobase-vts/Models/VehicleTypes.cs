using System;
using System.ComponentModel.DataAnnotations;

namespace autobase.Models
{
    public class VehicleTypes
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string VehicleName { get; set; }

        [Required, MaxLength(50)]
        public string VehicleType { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int CreatedBy { get; set; }  // Employee.Id of creator
    }
}
