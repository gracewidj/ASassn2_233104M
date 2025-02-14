using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ASassn2_233104M.Data;
using ASassn2_233104M.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Web;

namespace ASassn2_233104M.Pages
{
    [ValidateAntiForgeryToken] // CSRF Protection
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
            using (var aes = Aes.Create())
            {
                _key = aes.Key;
                _iv = aes.IV;
            }
        }

        [BindProperty]
        public User AppUser { get; set; } = new User(); // Ensure it's initialized

        [BindProperty]
        public IFormFile? UploadedPhoto { get; set; } 

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // 🔹 Sanitize User Inputs
            AppUser.Email = Regex.Replace(AppUser.Email.Trim().ToLower(), @"[^\w@.-]", "");
            AppUser.FullName = SanitizeInput(AppUser.FullName);
            AppUser.MobileNo = Regex.Replace(AppUser.MobileNo, @"\D", "");
            AppUser.DeliveryAddress = SanitizeInput(AppUser.DeliveryAddress);

            if (!string.IsNullOrEmpty(AppUser.AboutMe))
                AppUser.AboutMe = AppUser.AboutMe.Trim();

            // 🔹 Validate Email Format
            if (!new EmailAddressAttribute().IsValid(AppUser.Email))
            {
                ModelState.AddModelError("AppUser.Email", "Invalid email format.");
                return Page();
            }

            // 🔹 Check for duplicate email before saving
            if (await _context.Users.AnyAsync(u => u.Email == AppUser.Email))
            {
                ModelState.AddModelError("AppUser.Email", "Email already exists!");
                return Page();
            }

            // 🔹 Validate Password Strength
            string passwordFeedback = GetPasswordFeedback(AppUser.Password);
            if (!string.IsNullOrEmpty(passwordFeedback))
            {
                ModelState.AddModelError("AppUser.Password", passwordFeedback);
                return Page();
            }

            //  Assign PasswordSalt BEFORE Hashing
            AppUser.PasswordSalt = GenerateSalt();
            AppUser.Password = HashPassword(AppUser.Password, AppUser.PasswordSalt);

            // Encrypt Credit Card Number
            if (!string.IsNullOrWhiteSpace(AppUser.CreditCardNumber))
            {
                AppUser.CreditCardNumber = Convert.ToBase64String(EncryptData(AppUser.CreditCardNumber));
            }
            else
            {
                AppUser.CreditCardNumber = null; // Store null if empty
            }

            //  Handle File Upload 
            if (UploadedPhoto != null)
            {
                var allowedExtensions = new[] { ".jpg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(UploadedPhoto.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("UploadedPhoto", "Only JPG, PNG, and GIF images are allowed.");
                    return Page();
                }

                if (UploadedPhoto.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("UploadedPhoto", "File size must be under 5MB.");
                    return Page();
                }

                using (var memoryStream = new MemoryStream())
                {
                    await UploadedPhoto.CopyToAsync(memoryStream);
                    AppUser.Photo = memoryStream.ToArray();
                }
            }
            else
            {
                AppUser.Photo = null; 
            }

            // Default Security Settings
            AppUser.SessionToken = null;
            AppUser.FailedLoginAttempts = 0;
            AppUser.LockoutEnd = null;

            // Set Account Status to Pending (Requires Email Verification)
            AppUser.LockoutEnd = DateTime.UtcNow.AddMinutes(5); // Lock until email verification

            _context.Users.Add(AppUser);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Login");
        }

        // Function to Sanitize Regular Input Fields (Prevents XSS)
        private string SanitizeInput(string input)
        {
            return string.IsNullOrWhiteSpace(input) ? "" : HttpUtility.HtmlEncode(input.Trim());
        }

        // Function to Allow Special Characters in Email but Escape Others
        private string SanitizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "";

            email = email.Trim().ToLower();
            email = Regex.Replace(email, @"[^a-zA-Z0-9@._-]", ""); // Allow only email-friendly characters
            return email;
        }

        // Validate Password Complexity
        private string GetPasswordFeedback(string password)
        {
            List<string> errors = new List<string>();

            if (password.Length < 12)
                errors.Add("Must have at least 12 characters.");
            if (!Regex.IsMatch(password, @"[0-9]"))
                errors.Add("Must include at least one number.");
            if (!Regex.IsMatch(password, @"[a-z]"))
                errors.Add("Must include at least one lowercase letter.");
            if (!Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("Must include at least one uppercase letter.");
            if (!Regex.IsMatch(password, @"[$&+,:;=?@#|'<>.^*()%!-]"))
                errors.Add("Must include at least one special character.");

            return errors.Count > 0 ? "Password does not meet requirements: " + string.Join(" ", errors) : "";
        }

        // Generate Salt
        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        // 🔹 Hash Password with PBKDF2
        private string HashPassword(string password, string salt)
        {
            using (var rfc2898 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA512))
            {
                return Convert.ToBase64String(rfc2898.GetBytes(64));
            }
        }

        // Encrypt Sensitive Data (AES Encryption)
        private byte[] EncryptData(string data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] plainText = Encoding.UTF8.GetBytes(data);
                    return encryptor.TransformFinalBlock(plainText, 0, plainText.Length);
                }
            }
        }
    }
}