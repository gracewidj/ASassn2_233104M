using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASassn2_233104M.Data;
using System.Linq;

namespace ASassn2_233104M.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            string sessionToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(sessionToken))
            {
                return RedirectToPage("/Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null || user.SessionToken != sessionToken)
            {
                return RedirectToPage("/Login"); // Redirect to login if session invalid
            }

            return Page();
        }
    }
}