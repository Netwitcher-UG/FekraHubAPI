using Microsoft.AspNetCore.Http;
using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FekraHubAPI.Models.Users;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Authorization;

namespace FekraHubAPI.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class ManageController : ControllerBase
    {
        [HttpPost("[action]")]
        [Authorize(Roles =  ""+DefaultRole.Admin+","+DefaultRole.Secretariat )]
        public async Task<IActionResult> GetUser(int Id)
        {
            return Ok("a");
        }

    }


}
