using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ASassn2_233104M.Data;
using ASassn2_233104M.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ASassn2_233104M.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeSpan _minPasswordAge = TimeSpan.FromMinutes(5); // Prevents immediate password change
        private readonly TimeSpan _maxPasswordAge = TimeSpan.FromDays(90);   // Forces password update after 90 days

        public ChangePasswordModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ChangePasswordInputModel Input { get; set; } = new ChangePasswordInputModel();

        public string Message { get; set; }

        public class ChangePasswordInputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string CurrentPassword { get; set; }

            [Required, MinLength(12)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{12,}$",
            ErrorMessage = "Password must have at least 12 characters, including uppercase, lowercase, number, and special character.")]
            public string NewPassword { get; set; }

            [Required]
            [Compare("NewPassword")]
            public string ConfirmNewPassword { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
                return RedirectToPage("/Login");

            // 🔹 Check password age policies
            if (user.LastPasswordChange.HasValue)
            {
                TimeSpan timeSinceLastChange = DateTime.UtcNow - user.LastPasswordChange.Value;

                // Prevent password change if less than 5 minutes since last change
                if (timeSinceLastChange < _minPasswordAge)
                {
                    ModelState.AddModelError("", $"You must wait at least {Math.Ceiling(_minPasswordAge.TotalMinutes - timeSinceLastChange.TotalMinutes)} minutes before changing your password again.");
                    return Page();
                }

                // Force password update if it's older than 90 days
                if (timeSinceLastChange > _maxPasswordAge)
                {
                    ModelState.AddModelError("", "Your password has expired. Please change it now.");
                }
            }

            // 🔹 Verify old password
            string hashedCurrentPassword = HashPassword(Input.CurrentPassword, user.PasswordSalt);
            if (hashedCurrentPassword != user.Password)
            {
                ModelState.AddModelError("", "Current password is incorrect.");
                return Page();
            }

            // 🔹 Check password history (Cannot reuse last 2 passwords)
            if (user.PasswordHistory.Any(p => p == HashPassword(Input.NewPassword, user.PasswordSalt)))
            {
                ModelState.AddModelError("", "You cannot reuse the last 2 passwords. Please choose a different password.");
                return Page();
            }

            // 🔹 Update password history (Remove oldest if more than 2 entries)
            user.PasswordHistory.Add(user.Password);
            if (user.PasswordHistory.Count > 2)
                user.PasswordHistory.RemoveAt(0);

            // 🔹 Change Password
            user.Password = HashPassword(Input.NewPassword, user.PasswordSalt);
            user.LastPasswordChange = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            Message = "Password changed successfully!";
            return Page();
        }

        private string HashPassword(string password, string salt)
        {
            using (var rfc2898 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA512))
            {
                return Convert.ToBase64String(rfc2898.GetBytes(64));
            }
        }
    }
}