using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using ASassn2_233104M.Data;
using ASassn2_233104M.Models;
using System;
using System.Linq;

namespace ASassn2_233104M.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LogoutModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            string? userEmail = HttpContext.Session.GetString("UserEmail");

            if (!string.IsNullOrEmpty(userEmail))
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user != null)
                {
                    user.SessionToken = null;
                    _context.SaveChanges();
                }

                // Log audit only if email is not null
                LogAudit("Logout", userEmail);
            }

            HttpContext.Session.Clear();
            Response.Cookies.Delete("AuthToken");

            return RedirectToPage("/Login");
        }

        private void LogAudit(string action, string? email)
        {
            if (!string.IsNullOrEmpty(email)) // Ensure email is not null before logging
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
    }
}