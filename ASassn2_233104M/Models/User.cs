using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace ASassn2_233104M.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required, RegularExpression(@"^\d{8}$", ErrorMessage = "Mobile number must be exactly 8 digits.")]
        public string MobileNo { get; set; }

        [Required]
        public string DeliveryAddress { get; set; }

        [BindProperty, Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; }

        [Required, MinLength(12, ErrorMessage = "Password must be at least 12 characters.")]
        public string Password { get; set; }

        public string? PasswordSalt { get; set; }

        public string? CreditCardNumber { get; set; }

        public string? AboutMe { get; set; } 

        public byte[]? Photo { get; set; } 

        // 🔹 **Security Features**
        public string? SessionToken { get; set; } 
        public int FailedLoginAttempts { get; set; } = 0; 
        public DateTime? LockoutEnd { get; set; } 
    }
}
