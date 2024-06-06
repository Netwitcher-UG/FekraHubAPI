using FekraHubAPI.Data.Models;
using FekraHubAPI.Data;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FekraHubAPI.Services
{
    public class WorkContractService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public WorkContractService( UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        public static bool CheckCanDoWorkContract(string userRole)
        {
            List<string> CanDo = [DefaultRole.Admin, DefaultRole.Teacher, DefaultRole.Secretariat];
            if (CanDo.Contains(userRole))
            {
                return true;
            }
            return false;
        }
    }
}
