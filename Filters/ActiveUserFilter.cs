using CyberShield.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CyberShield.API.Filters
{
    public class ActiveUserFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _db;

        public ActiveUserFilter(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Only applies to authenticated requests — public endpoints pass through
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                await next();
                return;
            }

            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var user = await _db.Users.FindAsync(userId);
                if (user is null || !user.IsActive)
                {
                    context.Result = new ObjectResult(
                        new { message = "Account is disabled. Please contact support." })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return;
                }
            }

            await next();
        }
    }
}
