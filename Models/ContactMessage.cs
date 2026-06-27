using System;
using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.Models
{
    public class ContactMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        // تاريخ وقت إرسال الشكوى/الاستفسار تلقائياً
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // عشان الأدمن في الـ Dashboard يقدر يعلم عليها كـ "مقروءة" بعد ما يحلها
        public bool IsRead { get; set; } = false;
    }
}