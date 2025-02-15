using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ASassn2_233104M.Data;
using ASassn2_233104M.Models;
using System.Security.Cryptography;

namespace ASassn2_233104M.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ResetPasswordModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ResetPasswordInputModel Input { get; set; }

        [BindProperty]
        public string Token { get; set; }

        [BindProperty]
        public string Email { get; set; }

        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/ForgotPassword");
            }

            Token = token;
            Email = email;

            return Page();
        }
        private string HashPassword(string password, string salt)
        {
            using (var rfc2898 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA512))
            {
                return Convert.ToBase64String(rfc2898.GetBytes(64));
            }
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == Email && u.SessionToken == Token);
            if (user == null)
            {
                Message = "Invalid reset request.";
                return Page();
            }

            // Update password
            user.PasswordSalt = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            user.Password = HashPassword(Input.NewPassword, user.PasswordSalt);
            user.SessionToken = null; // Remove the reset token
            await _context.SaveChangesAsync();

            return RedirectToPage("/Login", new { Message = "Password reset successfully. You may now log in." });
        }
    }
}

public class ResetPasswordInputModel
{
    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
}

