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

        public async Task<IActionResult> Index()
        {
            try
            {
                ViewData["Layout"] = "~/Views/Shared/_AdminLayout.cshtml";

                var model = new AdminViewModel
                {
                    Users = await _context.Users
                        .OrderByDescending(u => u.CreatedAt)
                        .Take(10)
                        .ToListAsync(),
                    Events = await _context.Events
                        .Include(e => e.User)
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

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", id);
                    return NotFound();
                }

                ViewData["Layout"] = "~/Views/Shared/_AdminLayout.cshtml";
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user {UserId} for editing", id);
                return StatusCode(500, "An error occurred while loading the user for editing.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, User user, string newPassword)
        {
            try
            {
                if (id != user.UserId)
                {
                    return BadRequest();
                }

                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Update user properties
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.IsAdmin = user.IsAdmin;

                // Update password if provided
                if (!string.IsNullOrEmpty(newPassword))
                {
                    // Note: In production, you should hash the password here
                    existingUser.PasswordHash = newPassword;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} updated successfully", id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                ModelState.AddModelError("", "An error occurred while updating the user.");
                return View(user);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", id);
                    return NotFound();
                }

                ViewData["Layout"] = "~/Views/Shared/_AdminLayout.cshtml";
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user {UserId} for deletion", id);
                return StatusCode(500, "An error occurred while loading the user for deletion.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for deletion", id);
                    return NotFound();
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} deleted successfully", id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, "An error occurred while deleting the user.");
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
                ViewData["Layout"] = "~/Views/Shared/_AdminLayout.cshtml";

                var events = await _context.Events
                    .Include(e => e.User)
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
                ViewData["Layout"] = "~/Views/Shared/_AdminLayout.cshtml";

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

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            try
            {
                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem == null)
                {
                    _logger.LogWarning("Event {EventId} not found", id);
                    return NotFound();
                }

                ViewData["Layout"] = "~/Views/Shared/_AdminLayout.cshtml";
                return View(eventItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading event {EventId} for editing", id);
                return StatusCode(500, "An error occurred while loading the event for editing.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, Event eventModel)
        {
            try
            {
                if (id != eventModel.EventId)
                {
                    return BadRequest();
                }

                var existingEvent = await _context.Events.FindAsync(id);
                if (existingEvent == null)
                {
                    return NotFound();
                }

                // Update event properties
                existingEvent.EventName = eventModel.EventName;
                existingEvent.EventType = eventModel.EventType;
                existingEvent.Description = eventModel.Description;
                existingEvent.StartDateTime = eventModel.StartDateTime;
                existingEvent.EndDateTime = eventModel.EndDateTime;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Event {EventId} updated successfully", id);

                return RedirectToAction(nameof(Events));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event {EventId}", id);
                ModelState.AddModelError("", "An error occurred while updating the event.");
                return View(eventModel);
            }
        }

        [AllowAnonymous]
        [Route("admin/debug")]
        public async Task<IActionResult> DebugCheckData()
        {
            try
            {
                var data = new
                {
                    TotalEvents = await _context.Events.CountAsync(),
                    TotalVolunteers = await _context.Volunteers.CountAsync(),
                    TotalDonations = await _context.Donations.CountAsync(),
                    TotalMessages = await _context.ContactMessages.CountAsync(),
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