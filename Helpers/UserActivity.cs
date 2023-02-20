using DoctorWebApi.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DoctorWebApi.Helpers
{
    public class UserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            if (resultContext.HttpContext.User.Identity!.IsAuthenticated) return;

            var username = resultContext.HttpContext.User.Identity.Name;

            var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IAccountService>();

            var user  = await repo.GetUserAsync(username);  

            user.LastActive = DateTime.UtcNow;  

            await repo.SaveAllAsync();
        }
    }
}   
