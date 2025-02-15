using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ASassn2_233104M.Models;
using ASassn2_233104M.Data;
using Microsoft.EntityFrameworkCore;

namespace ASassn2_233104M.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ForgotPasswordModel(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [BindProperty]
        [Required, EmailAddress]
        public string Email { get; set; }

        public string Message { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
            if (user == null)
            {
                // Do not reveal that the email does not exist for security reasons
                Message = "If your email is registered, you will receive a password reset link.";
                return Page();
            }

            // Generate a reset token
            var token = Guid.NewGuid().ToString();
            user.SessionToken = token; // Store temporarily for password reset validation
            await _context.SaveChangesAsync();

            // Generate Reset Link
            var resetLink = Url.Page(
                "/ResetPassword",
                pageHandler: null,
                values: new { token = token, email = Email },
                protocol: Request.Scheme);

            // Send Email
            try
            {
                SendEmail(user.Email, "Reset Your Password", $"Click <a href='{HtmlEncoder.Default.Encode(resetLink)}'>here</a> to reset your password.");
                Message = "Password reset link sent. Please check your email.";
            }
            catch (Exception)
            {
                Message = "Error sending email. Try again later.";
            }

            return Page();
        }

        private void SendEmail(string email, string subject, string htmlMessage)
        {
            var smtpClient = new SmtpClient(_configuration["SMTP:Host"])
            {
                Port = int.Parse(_configuration["SMTP:Port"]),
                Credentials = new System.Net.NetworkCredential(_configuration["SMTP:Username"], _configuration["SMTP:Password"]),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["SMTP:SenderEmail"]),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);
            smtpClient.Send(mailMessage);
        }
    }
}
