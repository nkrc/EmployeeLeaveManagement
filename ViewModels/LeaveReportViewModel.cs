using EmployeeLeaveManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.ViewModels
{
    public class LeaveReportViewModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime? ToDate { get; set; }

        public string Status { get; set; } = "All";

        public List<LeaveRequest> LeaveRequests { get; set; }
            = new List<LeaveRequest>();
    }
}