using EmployeeLeaveManagement.Data;
using EmployeeLeaveManagement.Models;
using EmployeeLeaveManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmployeeLeaveManagement.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Employee Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var employeeId = GetCurrentEmployeeId();

            var requests = await _context.LeaveRequests
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.AppliedAt)
                .ToListAsync();

            ViewBag.TotalRequests = requests.Count;
            ViewBag.PendingRequests = requests.Count(l => l.Status == "Pending");
            ViewBag.ApprovedRequests = requests.Count(l => l.Status == "Approved");
            ViewBag.RejectedRequests = requests.Count(l => l.Status == "Rejected");

            ViewBag.PendingLeaves = requests
                .Where(l => l.Status == "Pending")
                .ToList();

            ViewBag.ApprovedLeaves = requests
                .Where(l => l.Status == "Approved")
                .ToList();

            ViewBag.RejectedLeaves = requests
                .Where(l => l.Status == "Rejected")
                .ToList();

            return View();
        }

        // Apply Leave - GET
        [HttpGet]
        public IActionResult ApplyLeave()
        {
            return View(new LeaveRequestViewModel
            {
                FromDate = DateTime.Today,
                ToDate = DateTime.Today
            });
        }

        // Apply Leave - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyLeave(LeaveRequestViewModel model)
        {
            var employeeId = GetCurrentEmployeeId();

            // Validate date range
            if (model.FromDate.Date > model.ToDate.Date)
            {
                TempData["ErrorMessage"] =
                    "From date cannot be later than To date.";

                return RedirectToAction(nameof(Dashboard));
            }

            // Check overlapping leave
            var hasOverlap = await _context.LeaveRequests.AnyAsync(l =>
                l.EmployeeId == employeeId &&
                (l.Status == "Pending" || l.Status == "Approved") &&
                model.FromDate.Date <= l.ToDate.Date &&
                model.ToDate.Date >= l.FromDate.Date
            );

            if (hasOverlap)
            {
                TempData["ErrorMessage"] =
                    "You already have a pending or approved leave request for these dates.";

                return RedirectToAction(nameof(Dashboard));
            }

            // Save leave request
            _context.LeaveRequests.Add(new LeaveRequest
            {
                EmployeeId = employeeId,
                FromDate = model.FromDate.Date,
                ToDate = model.ToDate.Date,
                Reason = model.Reason,
                Status = "Pending",
                AppliedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Leave request submitted successfully.";

            return RedirectToAction(nameof(Dashboard));
        }

        // My Leave Requests
        [HttpGet]
        public async Task<IActionResult> MyLeaves()
        {
            var employeeId = GetCurrentEmployeeId();

            var leaves = await _context.LeaveRequests
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.AppliedAt)
                .ToListAsync();

            return View(leaves);
        }

        // Withdraw Pending Leave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawLeave(int id)
        {
            var employeeId = GetCurrentEmployeeId();

            var leaveRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(l =>
                    l.Id == id &&
                    l.EmployeeId == employeeId);

            if (leaveRequest == null)
            {
                return NotFound();
            }

            if (leaveRequest.Status != "Pending")
            {
                TempData["ErrorMessage"] =
                    "Only pending leave requests can be withdrawn.";

                return RedirectToAction(nameof(MyLeaves));
            }

            leaveRequest.Status = "Withdrawn";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Leave request withdrawn successfully.";

            return RedirectToAction(nameof(MyLeaves));
        }

        private int GetCurrentEmployeeId()
        {
            var employeeIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.Parse(employeeIdValue!);
        }
    }
}