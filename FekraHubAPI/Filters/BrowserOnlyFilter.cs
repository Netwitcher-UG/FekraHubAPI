using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FekraHubAPI.Filters
{
    public class BrowserOnlyFilter : IAuthorizationFilter
    {
        private static readonly string[] _allowedBrowsers = new[]
        {
            "Mozilla", // Firefox
            "Chrome",  // Google Chrome
            "Safari",  // Safari
            "Edge",    // Microsoft Edge
            "Trident",  // Internet Explorer
            "PostmanRuntime"  // Postman
        };

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            if (request.Headers.ContainsKey("User-Agent"))
            {
                var userAgent = request.Headers["User-Agent"].ToString();

                if (!_allowedBrowsers.Any(browser => userAgent.Contains(browser)))
                {
                    context.Result = new ForbidResult("Access denied. Only browser requests are allowed.");
                }
            }
            else
            {
                context.Result = new ForbidResult("Access denied. Only browser requests are allowed.");
            }
        }
    }
}
