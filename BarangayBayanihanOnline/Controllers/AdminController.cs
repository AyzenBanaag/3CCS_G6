using BarangayBayanihanOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayBayanihanOnline.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation("DbSet initialization: Users={Users}, Events={Events}, Volunteers={Volunteers}, Donations={Donations}, ContactMessages={ContactMessages}, LoginHistory={LoginHistory}",
                _context.Users != null, _context.Events != null, _context.Volunteers != null, _context.Donations != null, _context.ContactMessages != null, _context.LoginHistory != null);
        }

        // In AdminController.cs
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new AdminViewModel
                {
                    Users = await _context.Users
                        .OrderByDescending(u => u.CreatedAt)
                        .Take(10)
                        .ToListAsync(),

                    Events = await _context.Events
                        .Include(e => e.User)  // This is crucial
                        .OrderByDescending(e => e.CreatedAt)
                        .Take(10)
                        .ToListAsync(),

                    Donations = await _context.Donations
                        .Include(d => d.User)
                        .OrderByDescending(d => d.DonationDate)
                        .Take(10)
                        .ToListAsync(),

                    ContactMessages = await _context.ContactMessages
                        .Include(c => c.User)
                        .OrderByDescending(m => m.SubmittedAt)
                        .Take(10)
                        .ToListAsync(),

                    Volunteers = await _context.Volunteers
                        .Include(v => v.User)
                        .OrderByDescending(v => v.ApplicationDate)
                        .Take(10)
                        .ToListAsync(),

                    LoginHistory = await _context.LoginHistory
                        .Include(l => l.User)
                        .OrderByDescending(l => l.LoginTime)
                        .Take(10)
                        .ToListAsync()
                };

                _logger.LogInformation("Admin dashboard loaded successfully");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                return StatusCode(500, "An error occurred while loading the dashboard.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyDonation(int donationId)
        {
            try
            {
                if (_context.Donations == null)
                {
                    _logger.LogError("Donations DbSet is null");
                    return StatusCode(500, "Database configuration error.");
                }
                var donation = await _context.Donations.FindAsync(donationId);
                if (donation == null)
                {
                    _logger.LogWarning("Donation {DonationId} not found", donationId);
                    return NotFound();
                }
                if (donation.Status == "Pending")
                {
                    donation.Status = "Verified";
                    int rowsAffected = await _context.SaveChangesAsync();
                    _logger.LogInformation("Donation {DonationId} verified. Rows affected: {RowsAffected}", donationId, rowsAffected);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying donation {DonationId}", donationId);
                return StatusCode(500, "An error occurred while verifying the donation.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveVolunteer(int volunteerId)
        {
            try
            {
                if (_context.Volunteers == null)
                {
                    _logger.LogError("Volunteers DbSet is null");
                    return StatusCode(500, "Database configuration error.");
                }
                var volunteer = await _context.Volunteers.FindAsync(volunteerId);
                if (volunteer == null)
                {
                    _logger.LogWarning("Volunteer {VolunteerId} not found", volunteerId);
                    return NotFound();
                }
                if (volunteer.Status == "Pending")
                {
                    volunteer.Status = "Approved";
                    int rowsAffected = await _context.SaveChangesAsync();
                    _logger.LogInformation("Volunteer {VolunteerId} approved. Rows affected: {RowsAffected}", volunteerId, rowsAffected);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving volunteer {VolunteerId}", volunteerId);
                return StatusCode(500, "An error occurred while approving the volunteer.");
            }
        }
        
        public async Task<IActionResult> Events()
        {
            try
            {
                var events = await _context.Events
                    .Include(e => e.User)  // Include user information
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                return View(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading events for admin");
                return StatusCode(500, "An error occurred while loading events.");
            }
        }

        public async Task<IActionResult> AllEvents()
        {
            try
            {
                var events = await _context.Events
                    .Include(e => e.User)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                return View(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading all events");
                return StatusCode(500, "An error occurred while loading events.");
            }
        }

        [AllowAnonymous] // Temporary - remove after debugging
        [Route("admin/debug")]
        public async Task<IActionResult> DebugCheckData()
        {
            try
            {
                var data = new
                {
                    // Basic counts
                    TotalEvents = await _context.Events.CountAsync(),
                    TotalVolunteers = await _context.Volunteers.CountAsync(),
                    TotalDonations = await _context.Donations.CountAsync(),
                    TotalMessages = await _context.ContactMessages.CountAsync(),

                    // Sample recent records
                    RecentEvents = await _context.Events
                        .Include(e => e.User)
                        .OrderByDescending(e => e.CreatedAt)
                        .Take(5)
                        .Select(e => new
                        {
                            e.EventId,
                            e.EventName,
                            e.StartDateTime,
                            User = e.User != null ? e.User.FullName : "null",
                            e.CreatedAt
                        })
                        .ToListAsync(),

                    RecentVolunteers = await _context.Volunteers
                        .Include(v => v.User)
                        .OrderByDescending(v => v.ApplicationDate)
                        .Take(5)
                        .Select(v => new
                        {
                            v.VolunteerId,
                            v.FullName,
                            v.Status,
                            User = v.User != null ? v.User.FullName : "null",
                            v.ApplicationDate
                        })
                        .ToListAsync(),

                    DatabaseConnection = await _context.Database.CanConnectAsync(),
                    LastMigration = await _context.Database.GetAppliedMigrationsAsync()
                };

                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}