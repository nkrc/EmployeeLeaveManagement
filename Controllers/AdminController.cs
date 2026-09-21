using EmployeeLeaveManagement.Data;
using EmployeeLeaveManagement.Models;
using EmployeeLeaveManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EmployeeLeaveManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalEmployees = await _context.Users
                .CountAsync(u => u.Role == "Employee" && u.IsActive);

            ViewBag.TotalLeaveRequests = await _context.LeaveRequests.CountAsync();

            ViewBag.PendingRequests = await _context.LeaveRequests.CountAsync(l => l.Status == "Pending");

            ViewBag.ApprovedRequests = await _context.LeaveRequests.CountAsync(l => l.Status == "Approved");

            ViewBag.RejectedRequests = await _context.LeaveRequests.CountAsync(l => l.Status == "Rejected");

            return View();
        }

        // Employee List
        [HttpGet]
        public async Task<IActionResult> Employees(string? search)
        {
            var query = _context.Users
                .Where(u => u.Role == "Employee")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
            }

            var employees = await query
                .OrderBy(u => u.Name)
                .Select(u => new EmployeeViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    TotalLeaveRequests = u.LeaveRequests.Count()
                })
                .ToListAsync();

            ViewBag.Search = search;

            return View(employees);
        }

        // Add Employee - GET
        [HttpGet]
        public IActionResult CreateEmployee()
        {
            return View();
        }

        // Add Employee - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(EmployeeViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Password is required.");
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email is already registered.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employee = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password!,
                Role = "Employee",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(employee);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee added successfully.";

            return RedirectToAction(nameof(Employees));
        }

        // Edit Employee - GET
        [HttpGet]
        public async Task<IActionResult> EditEmployee(int id)
        {
            var employee = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == id &&
                    u.Role == "Employee");

            if (employee == null)
            {
                return NotFound();
            }

            var model = new EmployeeViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                IsActive = employee.IsActive
            };

            return View(model);
        }

        // Edit Employee - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(
            int id,
            EmployeeViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var employee = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == id &&
                    u.Role == "Employee");

            if (employee == null)
            {
                return NotFound();
            }

            var emailExists = await _context.Users
                .AnyAsync(u =>
                    u.Email == model.Email &&
                    u.Id != id);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email is already registered.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            employee.Name = model.Name;
            employee.Email = model.Email;

            // Password is optional during edit.
            // If entered, update it.
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                employee.Password = model.Password;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee updated successfully.";

            return RedirectToAction(nameof(Employees));
        }

        // Deactivate Employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateEmployee(int id)
        {
            var employee = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == id &&
                    u.Role == "Employee");

            if (employee == null)
            {
                return NotFound();
            }

            employee.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee deactivated successfully.";

            return RedirectToAction(nameof(Employees));
        }

        [HttpGet]
        public async Task<IActionResult> LeaveRequests(string? status)
        {
            var query = _context.LeaveRequests
                .Include(l => l.Employee)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(l => l.Status == status);
            }

            var leaveRequests = await query
                .OrderByDescending(l => l.AppliedAt)
                .ToListAsync();

            ViewBag.SelectedStatus = status ?? "All";

            return View(leaveRequests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLeave(int id)
        {
            var leaveRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leaveRequest == null)
                return NotFound();

            if (leaveRequest.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending requests can be approved.";
                return RedirectToAction(nameof(LeaveRequests));
            }

            leaveRequest.Status = "Approved";
            leaveRequest.ReviewedAt = DateTime.UtcNow;
            leaveRequest.ReviewedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Leave request approved successfully.";

            return RedirectToAction(nameof(LeaveRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectLeave(int id)
        {
            var leaveRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leaveRequest == null)
                return NotFound();

            if (leaveRequest.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending requests can be rejected.";
                return RedirectToAction(nameof(LeaveRequests));
            }

            leaveRequest.Status = "Rejected";
            leaveRequest.ReviewedAt = DateTime.UtcNow;
            leaveRequest.ReviewedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Leave request rejected successfully.";

            return RedirectToAction(nameof(LeaveRequests));
        }

        [HttpGet]
        public async Task<IActionResult> Reports(
    DateTime? fromDate,
    DateTime? toDate,
    string status = "All")
        {
            var query = _context.LeaveRequests
                .Include(l => l.Employee)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(l =>
                    l.FromDate.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l =>
                    l.ToDate.Date <= toDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(l => l.Status == status);
            }

            var leaveRequests = await query
                .OrderByDescending(l => l.AppliedAt)
                .ToListAsync();

            var model = new LeaveReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                LeaveRequests = leaveRequests
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate, string status = "All")
        {
            var query = _context.LeaveRequests
                .Include(l => l.Employee)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(l =>
                    l.FromDate.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l =>
                    l.ToDate.Date <= toDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(l => l.Status == status);
            }

            var leaveRequests = await query
                .OrderByDescending(l => l.AppliedAt)
                .ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Leave Requests");

            worksheet.Cell(1, 1).Value = "Employee";
            worksheet.Cell(1, 2).Value = "Email";
            worksheet.Cell(1, 3).Value = "From Date";
            worksheet.Cell(1, 4).Value = "To Date";
            worksheet.Cell(1, 5).Value = "Reason";
            worksheet.Cell(1, 6).Value = "Applied At";
            worksheet.Cell(1, 7).Value = "Status";

            var headerRange = worksheet.Range("A1:G1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor =
                ClosedXML.Excel.XLColor.LightBlue;

            var row = 2;

            foreach (var leave in leaveRequests)
            {
                worksheet.Cell(row, 1).Value = leave.Employee?.Name;
                worksheet.Cell(row, 2).Value = leave.Employee?.Email;
                worksheet.Cell(row, 3).Value = leave.FromDate.ToString("dd-MM-yyyy");
                worksheet.Cell(row, 4).Value = leave.ToDate.ToString("dd-MM-yyyy");
                worksheet.Cell(row, 5).Value = leave.Reason;
                worksheet.Cell(row, 6).Value =
                    leave.AppliedAt.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt");
                worksheet.Cell(row, 7).Value = leave.Status;

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var fileName = $"LeaveReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(DateTime? fromDate, DateTime? toDate, string status = "All")
        {
            var query = _context.LeaveRequests
                .Include(l => l.Employee)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(l =>
                    l.FromDate.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l =>
                    l.ToDate.Date <= toDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(l => l.Status == status);
            }

            var leaveRequests = await query
                .OrderByDescending(l => l.AppliedAt)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);

                    page.Header()
                        .Text("Employee Leave Report")
                        .FontSize(20)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .PaddingTop(15)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.2f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Employee");
                                header.Cell().Element(HeaderStyle).Text("Email");
                                header.Cell().Element(HeaderStyle).Text("From Date");
                                header.Cell().Element(HeaderStyle).Text("To Date");
                                header.Cell().Element(HeaderStyle).Text("Reason");
                                header.Cell().Element(HeaderStyle).Text("Applied At");
                                header.Cell().Element(HeaderStyle).Text("Status");
                            });

                            foreach (var leave in leaveRequests)
                            {
                                table.Cell().Element(CellStyle)
                                    .Text(leave.Employee?.Name ?? "");

                                table.Cell().Element(CellStyle)
                                    .Text(leave.Employee?.Email ?? "");

                                table.Cell().Element(CellStyle)
                                    .Text(leave.FromDate.ToString("dd-MM-yyyy"));

                                table.Cell().Element(CellStyle)
                                    .Text(leave.ToDate.ToString("dd-MM-yyyy"));

                                table.Cell().Element(CellStyle)
                                    .Text(leave.Reason);

                                table.Cell().Element(CellStyle)
                                    .Text(leave.AppliedAt
                                        .ToLocalTime()
                                        .ToString("dd-MM-yyyy hh:mm tt"));

                                table.Cell().Element(CellStyle)
                                    .Text(leave.Status);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Generated on ");
                            text.Span(DateTime.Now.ToString("dd-MM-yyyy HH:mm"));
                        });
                });
            });

            var pdfBytes = document.GeneratePdf();

            var fileName = $"LeaveReport_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        static IContainer HeaderStyle(IContainer container)
        {
            return container
                .Background(Colors.Blue.Lighten2)
                .Padding(5)
                .DefaultTextStyle(x => x.Bold());
        }

        static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5);
        }
    }
}