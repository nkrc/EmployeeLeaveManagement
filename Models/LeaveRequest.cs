using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Pending";

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public int? ReviewedBy { get; set; }

        // Navigation property
        public User? Employee { get; set; }
    }
}