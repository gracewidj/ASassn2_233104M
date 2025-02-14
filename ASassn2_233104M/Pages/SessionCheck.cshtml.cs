using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASassn2_233104M.Pages
{
    public class SessionCheckModel : PageModel
    {
        public JsonResult OnGet()
        {
            try
            {
                bool isActive = HttpContext.Session.GetString("UserEmail") != null;
                return new JsonResult(new { isActive });
            }
            catch
            {
                return new JsonResult(new { isActive = false });
            }
        }
    }
}