using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace ASassn2_233104M.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full name can only contain letters and spaces.")]
        public string FullName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required, RegularExpression(@"^\d{8}$", ErrorMessage = "Mobile number must be exactly 8 digits.")]
        public string MobileNo { get; set; }

        [Required, RegularExpression(@"^[a-zA-Z0-9\s,.-]+$", ErrorMessage = "Delivery Address cannot contain special characters.")]
        public string DeliveryAddress { get; set; }

        [BindProperty, Required, EmailAddress, MaxLength(255), RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required, MinLength(12, ErrorMessage = "Password must be at least 12 characters.")]
        public string Password { get; set; }

        public string? PasswordSalt { get; set; }

        public string? CreditCardNumber { get; set; }

        public string? AboutMe { get; set; } 

        public byte[]? Photo { get; set; } 

        // Security Features
        public string? SessionToken { get; set; } 
        public int FailedLoginAttempts { get; set; } = 0; 
        public DateTime? LockoutEnd { get; set; }

        // Track Password History
        public List<string> PasswordHistory { get; set; } = new List<string>();

        // Track Last Password Change Date
        public DateTime? LastPasswordChange { get; set; }
    }
}
