using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Mvc.Filters;
namespace FekraHubAPI.Constract
{
    public class HandleLogFile 
    {

        public static string handleErrLogFile(ApplicationUser getCurrentAccount, string controller, string ErrMessage)
        {
            if (getCurrentAccount == null)
            {
                return "[IdUser: UserName:  Email:]"
                + " \nController:" + controller + " \nErrMessage:" + ErrMessage;
            }
            return "[IdUser:" + getCurrentAccount.Id +
                " \nUserName:" + getCurrentAccount.UserName + " \nEmail:" + getCurrentAccount.Email + "]"
                + " \nController:" + controller + " \nErrMessage:" + ErrMessage;

        }
    }
 
}
