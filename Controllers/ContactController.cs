using CyberShield.API.Data;
using CyberShield.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 📝 كلاس استقبال البيانات (Input DTO) مكتوب هنا داخلياً لمنع زحمة الملفات
        public class ContactFormInput
        {
            [Required(ErrorMessage = "الاسم بالكامل مطلوب")]
            [StringLength(100)]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
            [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
            [StringLength(150)]
            public string Email { get; set; } = string.Empty;

            [StringLength(20)]
            public string? PhoneNumber { get; set; }

            [Required(ErrorMessage = "عنوان الموضوع مطلوب")]
            [StringLength(200)]
            public string Subject { get; set; } = string.Empty;

            [Required(ErrorMessage = "نص الرسالة مطلوب")]
            public string Message { get; set; } = string.Empty;
        }

        // 1. مسار إرسال الرسالة (مفتوح للعامة في شاشة الـ Contact Us)
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ContactFormInput input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // تحويل البيانات الـ Model الحقيقي وتأمين الحقول الحساسة بالباك آند
            var newMessage = new ContactMessage
            {
                Name = input.Name,
                Email = input.Email,
                PhoneNumber = input.PhoneNumber,
                Subject = input.Subject,
                Message = input.Message,
                CreatedAt = DateTime.Now, // الوقت من السيرفر لضمان الأمان
                IsRead = false            // تنزل تلقائياً غير مقروءة
            };

            _context.ContactMessages.Add(newMessage);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إرسال رسالتك بنجاح! 🎉 سنقوم بمراجعتها والرد عليك في أقرب وقت." });
        }

        // 2. مسار جلب جميع الرسائل (محمي تماماً للأدمن فقط ليعرضها في الـ Dashboard)
        [HttpGet("all-messages")]
        [Authorize(Roles = "Admin")] // 🔥 لن يفتح إلا لمن يحمل توكن الأدمن
        public async Task<IActionResult> GetAllMessages()
        {
            // جلب الرسائل مرتبة من الأحدث للأقدم
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return Ok(messages);
        }

        // 3. مسار لتغيير حالة الرسالة إلى "مقروءة" (عندما يضغط الأدمن عليها في الـ Dashboard)
        [HttpPut("mark-as-read/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);

            if (message == null)
                return NotFound(new { message = "عذراً، هذه الرسالة غير موجودة!" });

            message.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تحديث حالة الرسالة إلى مقروءة بنجاح ✔️" });
        }
    }
}