using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
namespace FekraHubAPI.Constract
{
    public class HandleLogFile
    {

        public static string handleErrLogFile(ClaimsPrincipal User, string controller, string ErrMessage)
        {
            string userInfo = User == null
            ? "[IdUser: Email:]"
            : "[IdUser:" + User.FindFirst("id")?.Value + " \nEmail:" + User.FindFirst("name")?.Value + "]";

            return userInfo
                + " \nController:" + controller + " \nErrMessage:" + ErrMessage +
                "\n \n-----------------------------------------------------------------------------------\n";

        }
    }

}
