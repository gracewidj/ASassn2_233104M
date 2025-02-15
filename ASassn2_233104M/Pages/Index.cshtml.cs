using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASassn2_233104M.Data;
using ASassn2_233104M.Models;
using Microsoft.EntityFrameworkCore;

namespace ASassn2_233104M.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public User LoggedInUser { get; set; }

        public IActionResult OnGet()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Login"); // Redirect if user is not logged in
            }

            // Fetch user details from database
            LoggedInUser = _context.Users
                .AsNoTracking() // Prevents accidental modifications
                .FirstOrDefault(u => u.Email == userEmail);

            if (LoggedInUser == null)
            {
                return RedirectToPage("/Login"); // Redirect if user not found
            }

            return Page();
        }
    }
}