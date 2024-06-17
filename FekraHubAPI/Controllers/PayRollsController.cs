using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayRollsController : ControllerBase
    {
        private readonly IRepository<PayRoll> _payRollRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public PayRollsController(IRepository<PayRoll> payRollRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _payRollRepository = payRollRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> PostpayRoll( List<IFormFile> files)
        {
            string UserID = "17de782a-6b7a-4dfc-b007-dcc730694668";
            var user = await _userManager.FindByIdAsync(UserID);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var isTeacher = await _payRollRepository.IsTeacherIDExists(user.Id);
            var isSecretariat = await _payRollRepository.IsSecretariatIDExists(user);

            // var isSecretariat = await _userManager.IsInRoleAsync(user, DefaultRole.Secretariat);

            if (!(isTeacher || isSecretariat))
            {
                return BadRequest("User Must Have Teacher Or Secrtaria Role");
            }
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    byte[] fileBytes;
                    using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
                    }
                    var filePayRoll = fileBytes;
                    var UploadPayRoll = new Map_PayRoll
                    {
                        File = filePayRoll,
                        UserID = user.Id,

                    };

                    var PayRollEntity = _mapper.Map<PayRoll>(UploadPayRoll);
                    await _payRollRepository.Add(PayRollEntity);


                }
            }
            return Ok();

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayRoll(int id)
        {

            var PayRollEntity = await _payRollRepository.GetById(id);
            if (PayRollEntity == null)
            {
                return NotFound();
            }

            await _payRollRepository.Delete(id);
            return Ok();
        }




    }
}
