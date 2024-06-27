using AutoMapper;
using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Controllers.Excel_Migration
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelMigrationController : ControllerBase
    {
        private readonly IRepository<Student> _studentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public ExcelMigrationController(IRepository<Student> studentRepository,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db
             )
        {
            _db = db;
            _studentRepository = studentRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            
        }
       

       


        [HttpPost("UploadData")]
        public async Task<IActionResult> UploadData([Required] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is not selected or empty");

            

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    for (int row = 3; row <= 302; row++)
                    {

                    }
                        for (int row = 3; row <= 302; row++)
                    {
                        if (
                            !string.IsNullOrEmpty(worksheet.Cells[row, 2].Text) &&
                            !string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) &&
                            !string.IsNullOrEmpty(worksheet.Cells[row, 4].Text) &&
                            !string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) &&
                            !string.IsNullOrEmpty(worksheet.Cells[row, 6].Text) &&
                            !string.IsNullOrEmpty(worksheet.Cells[row, 11].Text) &&
                            !string.IsNullOrEmpty(worksheet.Cells[row, 13].Text)
                            )
                        {

                                string RoleParent = DefaultRole.Parent;
                                var email = worksheet.Cells[row, 13].Text;
                            var user = new ApplicationUser
                            {
                                UserName =  worksheet.Cells[row, 11].Text ,// Name as user name
                                FirstName = worksheet.Cells[row, 11].Text,
                                LastName = worksheet.Cells[row, 12].Text,
                                Email = email,
                                Birthday = string.IsNullOrEmpty(worksheet.Cells[row, 14].Text) ? (DateTime?)null : DateTime.Parse(worksheet.Cells[row, 14].Text),
                                Birthplace = worksheet.Cells[row, 15].Text,
                                Nationality = worksheet.Cells[row, 16].Text,
                                PhoneNumber = worksheet.Cells[row, 17].Text,
                                EmergencyPhoneNumber = worksheet.Cells[row, 18].Text,
                                Gender = worksheet.Cells[row, 19].Text,
                                City = worksheet.Cells[row, 20].Text,
                                Street = worksheet.Cells[row, 21].Text,
                                StreetNr = worksheet.Cells[row, 22].Text,
                                ZipCode = worksheet.Cells[row, 23].Text,
                                Job = worksheet.Cells[row, 24].Text,
                                Graduation = worksheet.Cells[row, 25].Text,
                                SecurityStamp = Guid.NewGuid().ToString("D"),
                                NormalizedUserName = email.ToUpper(),
                                NormalizedEmail = email.ToUpper(),
                                EmailConfirmed = true,
                                ActiveUser = true
                            };
                            var UserExists = await _userManager.FindByEmailAsync(email);
                            var UserNameExists = await _userManager.FindByNameAsync(user.UserName);
                            if (UserExists == null && UserNameExists == null)
                            {
                                using (IDbContextTransaction transaction = _db.Database.BeginTransaction())
                                {
                                    IdentityResult result = await _userManager.CreateAsync(user, "FekraSchule.2024");
                                    if (!result.Succeeded)
                                    {
                                        await transaction.RollbackAsync();
                                        return BadRequest(result.Errors);
                                    }

                                    var roleResult = await _userManager.AddToRoleAsync(user, DefaultRole.Parent);
                                    if (!roleResult.Succeeded)
                                    {
                                        await transaction.RollbackAsync();
                                        return BadRequest(roleResult.Errors);
                                    }

                                    await transaction.CommitAsync();

                                }
                                var student = new Student
                                {
                                    FirstName = worksheet.Cells[row, 2].Text,
                                    LastName = worksheet.Cells[row, 3].Text,
                                    Birthday = DateTime.Parse(worksheet.Cells[row, 4].Text),
                                    Nationality = worksheet.Cells[row, 5].Text,
                                    Note = worksheet.Cells[row, 6].Text,
                                    City = worksheet.Cells[row, 7].Text,
                                    Street = worksheet.Cells[row, 8].Text,
                                    StreetNr = worksheet.Cells[row, 9].Text,
                                    ZipCode = worksheet.Cells[row, 10].Text,
                                    ParentID = UserExists == null ? user.Id : UserExists.Id

                                };
                                await _studentRepository.Add(student);

                            }





                        }
                    }
                }

                return Ok("Success");
            }

        }


    }
}
