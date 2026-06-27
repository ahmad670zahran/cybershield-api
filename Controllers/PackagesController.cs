using CyberShield.API.DTOs;
using CyberShield.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesController : ControllerBase
    {
        private readonly IPackageService _packageService;

        public PackagesController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var packages = await _packageService.GetAllAsync();
            return Ok(packages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var package = await _packageService.GetByIdAsync(id);
            if (package is null)
                return NotFound(new { message = "Package not found." });

            return Ok(package);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePackageDto dto)
        {
            var created = await _packageService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePackageDto dto)
        {
            var updated = await _packageService.UpdateAsync(id, dto);
            if (updated is null)
                return NotFound(new { message = "Package not found." });

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _packageService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = "Package not found." });

            return Ok(new { message = "Package deactivated successfully." });
        }
    }
}
