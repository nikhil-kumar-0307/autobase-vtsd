namespace autobase.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using autobase.Models;
    using autobase.Helpers;
    using autobase.Data;

    internal sealed class Configuration : DbMigrationsConfiguration<AutobaseDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(AutobaseDbContext context)
        {
            context.Employees.AddOrUpdate(e => e.EmployeeNumber,
                new Employee
                {
                    EmployeeNumber = "1000",
                    FullName = "Super Admin",
                    Email = "superadmin@autobase.com",
                    PasswordHash = PasswordHelper.HashPassword("SuperAdmin@1234"),
                    Role = "SuperAdmin",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Employee
                {
                    EmployeeNumber = "1010",
                    FullName = "HOD",
                    Email = "HOD@autobase.com",
                    PasswordHash = PasswordHelper.HashPassword("HOD@1234"),
                    Role = "HOD",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Employee
                {
                    EmployeeNumber = "1001",
                    FullName = "Admin User",
                    Email = "admin@autobase.com",
                    PasswordHash = PasswordHelper.HashPassword("Admin@1234"),
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Employee
                {
                    EmployeeNumber = "1002",
                    FullName = "John Employee",
                    Email = "john@autobase.com",
                    PasswordHash = PasswordHelper.HashPassword("Employee@1234"),
                    Role = "Employee",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );

            context.SaveChanges();
        }
    
    }
}