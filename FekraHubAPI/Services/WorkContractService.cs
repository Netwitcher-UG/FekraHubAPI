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

        public static bool  CheckUserWorkContract(string userRole)
        {
            List<string> RoleWorkContract = [DefaultRole.Teacher, DefaultRole.Secretariat];
            if (RoleWorkContract.Contains(userRole))
            {
                return true;
            }
            return false;
        }
    }
}
