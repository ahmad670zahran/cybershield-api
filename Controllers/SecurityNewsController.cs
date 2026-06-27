using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CyberShield.API.Data; // عشان يقدر يشوف الـ ApplicationDbContext

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityNewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SecurityNewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // الأكشن المسؤول عن جلب الأخبار مرتبة من الأحدث للأقدم
        [HttpGet]
        public async Task<IActionResult> GetAllNews()
        {
            var news = await _context.SecurityNews
                                     .OrderByDescending(n => n.Id)
                                     .ToListAsync();
            return Ok(news);
        }
    }
}