using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CyberShield.API.Data; // عشان يقدر يشوف الـ ApplicationDbContext

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityTipsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SecurityTipsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // الأكشن المسؤول عن جلب كل النصائح الأمنية للشاشة
        [HttpGet]
        public async Task<IActionResult> GetAllTips()
        {
            var tips = await _context.SecurityTips.ToListAsync();
            return Ok(tips);
        }
    }
}