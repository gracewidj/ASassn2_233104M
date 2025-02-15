
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASassn2_233104M.Data;
using ASassn2_233104M.Models;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace ASassn2_233104M.Pages
{


    public class LoginModel : PageModel
    {
        public void OnGet()
        {
            Console.WriteLine("Login page loaded");
        }

        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(ApplicationDbContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty, Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; }

        [BindProperty, Required, MinLength(12), MaxLength(128)]
        public string Password { get; set; }

        [BindProperty]
        public string RecaptchaResponse { get; set; }

        private const int MaxFailedAttempts = 3;
        private const int LockoutTimeMinutes = 15;

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("Login function triggered" + RecaptchaResponse);

            if (!ModelState.IsValid)
                return Page();

            Console.WriteLine($"RecaptchaResponse: {RecaptchaResponse}");

            //if (string.IsNullOrEmpty(RecaptchaResponse))
            //{
            //    ModelState.AddModelError("", "reCAPTCHA response is missing.");
            //    return Page();
            //}

            //// Validate reCAPTCHA before login
            //bool isRecaptchaValid = await ValidateRecaptchaAsync(RecaptchaResponse);
            //if (!isRecaptchaValid)
            //{
            //    LogAudit("Failed reCAPTCHA Validation", Email);
            //    ModelState.AddModelError("", "reCAPTCHA verification failed. Please try again.");
            //    return Page();
            //}

            // Sanitize Email Input
            Email = Email.Trim().ToLower();

            Console.WriteLine("🔹 Login Attempt: " + Email);

            var user = _context.Users.FirstOrDefault(u => u.Email == Email);

            if (user == null)
            {
                LogAudit("Failed Login - User Not Found", Email);
                Console.WriteLine("User Not Found in Database: " + Email);
                ModelState.AddModelError("", "Invalid email or password.");
                return Page();
            }
            else
            {
                Console.WriteLine($"User found: {user.Email}");
            }

            // Check if account is locked
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                LogAudit("Failed Login - Account Locked", Email);
                ModelState.AddModelError("", $"Your account is locked. Try again after {LockoutTimeMinutes} minutes.");
                return Page();
            }

            // Validate Password
            string hashedInputPassword = HashPassword(Password, user.PasswordSalt);
            if (hashedInputPassword != user.Password)
            {
                // Handle failed login attempts
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutTimeMinutes);
                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    LogAudit("Account Locked Due to Failed Logins", Email);
                    ModelState.AddModelError("", $"Your account is locked due to too many failed login attempts. Try again after {LockoutTimeMinutes} minutes.");
                    return Page();
                }

                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                LogAudit("Failed Login - Incorrect Password", Email);
                ModelState.AddModelError("", "Invalid email or password.");
                return Page();
            }

            // Reset failed login attempts on successful login
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null; 
            user.SessionToken = Guid.NewGuid().ToString();
            await _context.SaveChangesAsync();

            // Check if the user is already logged in from another device
            if (!string.IsNullOrEmpty(user.SessionToken))
            {
                Console.WriteLine("Existing session detected, logging out previous session.");

                // Invalidate the existing session token in the database
                user.SessionToken = null;
                _context.SaveChanges();
            }

            // Generate a new session token for this login session
            user.SessionToken = Guid.NewGuid().ToString();
            _context.SaveChanges();

            // Store session in HttpContext
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("AuthToken", user.SessionToken);
            Response.Cookies.Append("AuthToken", user.SessionToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            LogAudit("Successful Login", Email);

            return RedirectToPage("/Index");
        }

            private string HashPassword(string password, string salt)
        {
            using (var rfc2898 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA512))
            {
                return Convert.ToBase64String(rfc2898.GetBytes(64));
            }
        }

        private async Task<bool> ValidateRecaptchaAsync(string recaptchaResponse)
        {
            string secretKey = _configuration["GoogleReCAPTCHA:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("Google reCAPTCHA Secret Key is missing in configuration.");
            }

            var client = _httpClientFactory.CreateClient();
            var parameters = new Dictionary<string, string>
            {
                { "secret", secretKey },
                { "response", recaptchaResponse }
            };

            var encodedContent = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", encodedContent);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var recaptchaResult = JsonSerializer.Deserialize<ReCaptchaResponse>(jsonResponse);

            return recaptchaResult?.Success == true && recaptchaResult.Score >= 0.5;
        }

        private void LogAudit(string action, string email)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                Email = email,
                Action = action,
                Timestamp = DateTime.UtcNow
            });
            _context.SaveChanges();
        }
    }

    public class ReCaptchaResponse
    {
        public bool Success { get; set; }
        public double Score { get; set; }
    }
}