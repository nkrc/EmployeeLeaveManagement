using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.ViewModels
{
    public class LeaveRequestViewModel
    {
        [Required(ErrorMessage = "From date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime FromDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "To date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime ToDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Reason is required.")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Reason { get; set; } = string.Empty;
    }
}