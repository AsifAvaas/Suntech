using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using static SunTech.Models.DatabaseModel;
using SunTech.Models;
using SunTech.Data;
using Microsoft.EntityFrameworkCore;

namespace SunTech.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null && VerifyPassword(model.Password, user.PasswordHash))
                {
                    var claims = new List<Claim>
{
new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
new Claim(ClaimTypes.Name, user.Name),
new Claim(ClaimTypes.Email, user.Email),
new Claim(ClaimTypes.Role, user.Role)
};

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                    _logger.LogInformation($"User {user.Email} logged in successfully");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "An account with this email already exists.");
                    return View(model);
                }

                // Create new user
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Role = model.Role,
                    PasswordHash = HashPassword(model.Password)
                };

                try
                {
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"New user registered: {user.Email}");

                    // Auto login after registration
                    var claims = new List<Claim>
{
new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
new Claim(ClaimTypes.Name, user.Name),
new Claim(ClaimTypes.Email, user.Email),
new Claim(ClaimTypes.Role, user.Role)
};

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                    TempData["SuccessMessage"] = "Account created successfully! Welcome to SunTech.";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user account");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating your account. Please try again.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation($"User {userEmail} logged out");

            TempData["InfoMessage"] = "You have been successfully logged out.";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper methods for password hashing
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var salt = GenerateSalt();
                var saltedPassword = password + salt;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes) + ":" + salt;
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                var parts = hash.Split(':');
                if (parts.Length != 2) return false;

                var storedHash = parts[0];
                var salt = parts[1];

                using (var sha256 = SHA256.Create())
                {
                    var saltedPassword = password + salt;
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                    var computedHash = Convert.ToBase64String(hashedBytes);
                    return computedHash == storedHash;
                }
            }
            catch
            {
                return false;
            }
        }

        private string GenerateSalt()
        {
            var saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }
    }

    // Additional ViewModel for Profile
    public class ProfileViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Role")]
        public string Role { get; set; }
    }
}

