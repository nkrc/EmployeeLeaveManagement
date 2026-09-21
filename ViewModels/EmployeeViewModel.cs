using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.ViewModels
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool IsActive { get; set; } = true;

        public int TotalLeaveRequests { get; set; }
    }
}