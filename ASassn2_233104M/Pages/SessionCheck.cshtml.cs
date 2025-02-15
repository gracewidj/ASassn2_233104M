using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASassn2_233104M.Data;
using System.Linq;

namespace ASassn2_233104M.Pages
{
    public class SessionCheckModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SessionCheckModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public JsonResult OnGet()
        {
            try
            {
                string? userEmail = HttpContext.Session.GetString("UserEmail");
                string? authToken = HttpContext.Session.GetString("AuthToken");

                if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(authToken))
                {
                    return new JsonResult(new { isActive = false });
                }

                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null || user.SessionToken != authToken)
                {
                    // If session token in DB does not match the current session, force logout
                    HttpContext.Session.Clear();
                    Response.Cookies.Delete("AuthToken");
                    return new JsonResult(new { isActive = false });
                }

                return new JsonResult(new { isActive = true });
            }
            catch
            {
                return new JsonResult(new { isActive = false });
            }
        }
    }
}