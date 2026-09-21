using EmployeeLeaveManagement.Models;

namespace EmployeeLeaveManagement.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {            
            if (context.Users.Any())
            {
                return;
            }

            var admin = new User
            {
                Name = "Admin User",
                Email = "admin@example.com",
                Password = "admin123",
                Role = "Admin",
                IsActive = true
            };

            var employee = new User
            {
                Name = "Employee User",
                Email = "employee@example.com",
                Password = "emp123",
                Role = "Employee",
                IsActive = true
            };

            context.Users.AddRange(admin, employee);

            context.SaveChanges();
        }
    }
}