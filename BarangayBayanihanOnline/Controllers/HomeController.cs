using BarangayBayanihanOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace BarangayBayanihanOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation("DbSet initialization: Users={Users}, Events={Events}, Volunteers={Volunteers}, Donations={Donations}, ContactMessages={ContactMessages}, Posts={Posts}",
                _context.Users != null, _context.Events != null, _context.Volunteers != null, _context.Donations != null, _context.ContactMessages != null, _context.Posts != null);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Mission()
        {
            return View();
        }

        public IActionResult OurProgram()
        {
            return View();
        }

        [HttpGet]
        public IActionResult MyEvents()
        {
            return View("MyEvents", new Event());
        }
        [HttpPost]
        public async Task<IActionResult> MyEvents(Event model)
        {
            if (model == null)
            {
                _logger.LogError("Event model is null in CreateEvent action");
                return BadRequest("Invalid form data.");
            }

            _logger.LogInformation("Creating event: EventName={EventName}, StartDateTime={StartDateTime}", model.EventName, model.StartDateTime);

            if (ModelState.IsValid)
            {
                try
                {
                    if (_context.Events == null)
                    {
                        _logger.LogError("Events DbSet is null");
                        return StatusCode(500, "Database configuration error.");
                    }
                    model.UserId = await GetUserIdAsync();
                    model.CreatedAt = EnsureValidDateTime(DateTime.Now, "CreatedAt");
                    _context.Events.Add(model);
                    int rowsAffected = await _context.SaveChangesAsync();
                    _logger.LogInformation("Event saved successfully. Rows affected: {RowsAffected}", rowsAffected);
                    TempData["Message"] = "Event created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating event {EventName}", model.EventName ?? "unknown");
                    ModelState.AddModelError("", "An error occurred while creating the event. Please try again.");
                }
            }
            else
            {
                LogModelStateErrors("event");
            }
            return View("MyEvents", model);
        }

        [HttpGet]
        public IActionResult ContactUs()
        {
            return View(new ContactMessage());
        }

        [HttpPost]
        public async Task<IActionResult> Submit(ContactMessage model)
        {
            if (model == null)
            {
                _logger.LogError("ContactMessage model is null in Submit action");
                return BadRequest("Invalid form data.");
            }

            _logger.LogInformation("Submitting contact message: Name={Name}, Email={Email}, Subject={Subject}", model.Name, model.Email, model.Subject);

            if (ModelState.IsValid)
            {
                try
                {
                    if (_context.ContactMessages == null)
                    {
                        _logger.LogError("ContactMessages DbSet is null");
                        return StatusCode(500, "Database configuration error.");
                    }
                    model.UserId = await GetUserIdAsync();
                    model.SubmittedAt = EnsureValidDateTime(DateTime.Now, "SubmittedAt");
                    model.Status = "New";
                    _context.ContactMessages.Add(model);
                    int rowsAffected = await _context.SaveChangesAsync();
                    _logger.LogInformation("Contact message saved successfully. Rows affected: {RowsAffected}", rowsAffected);
                    TempData["MessageSent"] = "Your message has been sent successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving contact message for {Email}", model.Email ?? "unknown");
                    ModelState.AddModelError("", "An error occurred while saving your message. Please try again.");
                }
            }
            else
            {
                LogModelStateErrors("contact message");
            }
            return View("ContactUs", model);
        }

        [HttpGet]
        public IActionResult Volunteer()
        {
            return View(new Volunteer());
        }

        [HttpPost]
        public async Task<IActionResult> Volunteer(Volunteer model)
        {
            if (model == null)
            {
                _logger.LogError("Volunteer model is null in Volunteer action");
                return BadRequest("Invalid form data.");
            }

            _logger.LogInformation("Submitting volunteer application: FullName={FullName}, Email={Email}", model.FullName, model.Email);

            if (ModelState.IsValid)
            {
                try
                {
                    if (_context.Volunteers == null)
                    {
                        _logger.LogError("Volunteers DbSet is null");
                        return StatusCode(500, "Database configuration error.");
                    }
                    model.UserId = await GetUserIdAsync();
                    model.ApplicationDate = EnsureValidDateTime(DateTime.Now, "ApplicationDate");
                    model.Status = "Pending";
                    _context.Volunteers.Add(model);
                    int rowsAffected = await _context.SaveChangesAsync();
                    _logger.LogInformation("Volunteer application saved successfully. Rows affected: {RowsAffected}", rowsAffected);
                    TempData["Message"] = "Volunteer application submitted successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error submitting volunteer application for {Email}", model.Email ?? "unknown");
                    ModelState.AddModelError("", "An error occurred while submitting your application. Please try again.");
                }
            }
            else
            {
                LogModelStateErrors("volunteer");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Donate()
        {
            return View(new Donation());
        }

        [HttpPost]
        public async Task<IActionResult> Donate(Donation model)
        {
            if (model == null)
            {
                _logger.LogError("Donation model is null in Donate action");
                return BadRequest("Invalid form data.");
            }

            _logger.LogInformation("Submitting donation: DonorName={DonorName}, Amount={Amount}", model.DonorName, model.Amount);

            if (ModelState.IsValid)
            {
                try
                {
                    if (_context.Donations == null)
                    {
                        _logger.LogError("Donations DbSet is null");
                        return StatusCode(500, "Database configuration error.");
                    }
                    model.UserId = await GetUserIdAsync();
                    model.DonationDate = EnsureValidDateTime(DateTime.Now, "DonationDate");
                    model.Status = "Pending";
                    _context.Donations.Add(model);
                    int rowsAffected = await _context.SaveChangesAsync();
                    _logger.LogInformation("Donation saved successfully. Rows affected: {RowsAffected}", rowsAffected);
                    TempData["Message"] = "Donation submitted successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error submitting donation for {DonorName}", model.DonorName ?? "unknown");
                    ModelState.AddModelError("", "An error occurred while submitting your donation. Please try again.");
                }
            }
            else
            {
                LogModelStateErrors("donation");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Login attempt with empty email or password");
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            try
            {
                if (_context.Users == null)
                {
                    _logger.LogError("Users DbSet is null");
                    return StatusCode(500, "Database configuration error.");
                }
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
                if (user == null || !VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid login attempt for {Email}", email);
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View();
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = rememberMe
                });

                var loginTime = EnsureValidDateTime(DateTime.Now, "LastLogin");
                user.LastLogin = loginTime;
                await _context.LoginHistory.AddAsync(new LoginHistory
                {
                    UserId = user.UserId,
                    LoginTime = loginTime,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });
                int rowsAffected = await _context.SaveChangesAsync();
                _logger.LogInformation("User {Email} logged in. Rows affected: {RowsAffected}", user.Email, rowsAffected);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", email);
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("User logged out");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Signup(string fullName, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                _logger.LogWarning("Signup attempt with empty fields");
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            if (password != confirmPassword)
            {
                _logger.LogWarning("Signup passwords do not match for {Email}", email);
                ModelState.AddModelError("", "Passwords do not match.");
                return View();
            }

            try
            {
                if (_context.Users == null)
                {
                    _logger.LogError("Users DbSet is null");
                    return StatusCode(500, "Database configuration error.");
                }
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Email {Email} already registered", email);
                    ModelState.AddModelError("", "Email already registered.");
                    return View();
                }

                var user = new User
                {
                    FullName = fullName,
                    Email = email,
                    PasswordHash = HashPassword(password),
                    CreatedAt = EnsureValidDateTime(DateTime.Now, "CreatedAt"),
                    IsActive = true,
                    IsAdmin = false
                };
                await _context.Users.AddAsync(user);
                int rowsAffected = await _context.SaveChangesAsync();
                _logger.LogInformation("User {Email} signed up. Rows affected: {RowsAffected}", email, rowsAffected);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signup for {Email}", email);
                ModelState.AddModelError("", "An error occurred during signup. Please try again.");
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("ForgotPassword attempt with empty email");
                ModelState.AddModelError("", "Email is required.");
                return View("Login");
            }

            try
            {
                if (_context.Users == null)
                {
                    _logger.LogError("Users DbSet is null");
                    return StatusCode(500, "Database configuration error.");
                }
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    user.PasswordResetToken = Guid.NewGuid().ToString();
                    user.PasswordResetTokenExpiry = EnsureValidDateTime(DateTime.Now.AddHours(1), "PasswordResetTokenExpiry");
                    int rowsAffected = await _context.SaveChangesAsync();
                    _logger.LogInformation("Password reset requested for {Email}. Rows affected: {RowsAffected}", email, rowsAffected);
                    // TODO: Send reset email
                }
                TempData["Message"] = "If the email exists, a reset link has been sent.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for {Email}", email);
                TempData["Message"] = "An error occurred. Please try again.";
                return RedirectToAction("Login");
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task<int?> GetUserIdAsync()
        {
            if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(User.Identity.Name))
            {
                _logger.LogWarning("User is not authenticated or User.Identity.Name is null");
                return null;
            }

            try
            {
                if (_context.Users == null)
                {
                    _logger.LogError("Users DbSet is null");
                    return null;
                }
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
                if (user == null)
                {
                    _logger.LogWarning("User not found for email {Email}", User.Identity.Name);
                    return null;
                }
                _logger.LogInformation("Retrieved UserId {UserId} for email {Email}", user.UserId, User.Identity.Name);
                return user.UserId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving UserId for {Email}", User.Identity.Name);
                return null;
            }
        }

        private void LogModelStateErrors(string formType)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is valid for {FormType}", formType);
                return;
            }

            try
            {
                var errors = new List<string>();
                if (!ModelState.Values.Any())
                {
                    _logger.LogWarning("ModelState.Values is empty for {FormType}", formType);
                    errors.Add("ModelState values are missing.");
                }

                else
                {
                    foreach (var value in ModelState.Values)
                    {
                        if (value.Errors == null)
                        {
                            _logger.LogWarning("ModelState.Errors is null for {FormType}", formType);
                            continue;
                        }
                        if (value.Errors.Any())
                        {
                            errors.AddRange(value.Errors.Select(e => e.ErrorMessage ?? "Unknown error"));
                        }
                    }
                }

                if (errors.Any())
                {
                    _logger.LogWarning("Invalid model state for {FormType}: {Errors}", formType, string.Join("; ", errors));
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid model state for {FormType}, but no specific errors found", formType);
                    ModelState.AddModelError("", "Invalid form data. Please check your inputs.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging ModelState errors for {FormType}", formType);
                ModelState.AddModelError("", "An error occurred while validating the form. Please try again.");
            }
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                _logger.LogError("Password is null or empty in HashPassword");
                throw new ArgumentNullException(nameof(password));
            }
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            {
                _logger.LogWarning("Password or hash is null in VerifyPassword");
                return false;
            }
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private DateTime EnsureValidDateTime(DateTime dateTime, string propertyName)
        {
            var minDate = new DateTime(1753, 1, 1);
            var maxDate = new DateTime(9999, 12, 31, 23, 59, 59, 997);

            if (dateTime < minDate)
            {
                _logger.LogWarning($"DateTime value for {propertyName} ({dateTime}) is before 1753-01-01. Setting to minimum: {minDate}");
                return minDate;
            }
            if (dateTime > maxDate)
            {
                _logger.LogWarning($"DateTime value for {propertyName} ({dateTime}) is after 9999-12-31. Setting to maximum: {maxDate}");
                return maxDate;
            }
            return dateTime;
        }
    }
}