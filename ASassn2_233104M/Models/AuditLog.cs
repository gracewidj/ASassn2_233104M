using System;
using System.ComponentModel.DataAnnotations;

namespace ASassn2_233104M.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public string?Email { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty; // Ensure it's never mull

        public DateTime Timestamp { get; set; }
    }
}