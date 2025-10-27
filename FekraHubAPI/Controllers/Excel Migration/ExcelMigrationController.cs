using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using FekraHubAPI.Constract;

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
        private readonly ILogger<ExcelMigrationController> _logger;
        private readonly EmailSender.IEmailSender _emailSender;
        public ExcelMigrationController(IRepository<Student> studentRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, ApplicationDbContext db,
            ILogger<ExcelMigrationController> logger, EmailSender.IEmailSender emailSender
             )
        {
            _db = db;
            _studentRepository = studentRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        //[Authorize(Policy = "ManageExcelMigration")]
        [HttpPost("UploadData")]
        public async Task<IActionResult> UploadData([Required] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("File is not selected or empty");

                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var count = 0;
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];

                        for (int row = 3; row <= 302; row++)
                        {
                            var ex = ExcelExceptions(worksheet, row, emailRegex);
                            if (ex.Count > 0 && ex.Count < 7)
                            {
                                return BadRequest(ex);
                            }
                            
                            //var dateP = worksheet.Cells[row, 15].Text;
                            //var dateS = worksheet.Cells[row, 4].Text;

                        }
                        for (int row = 3; row <= 302; row++)
                        {
                            if (IsRowValid(worksheet, row))
                            {
                                var email = worksheet.Cells[row, 14].Text.Trim().Replace(" ", "");
                                var user = await GetUserAsync(email, worksheet, row);

                                if (user != null)
                                {
                                    var student = CreateStudent(worksheet, row, user.Id);
                                    await _studentRepository.Add(student);
                                    count++;
                                }
                            }
                            
                        }
                    }
                }

                return Ok($"{count} students have been added");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ExcelMigrationController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        private List<string> ExcelExceptions(ExcelWorksheet worksheet, int row, Regex regex)
        {
            if(IsRowValid(worksheet, row))
            {
                return new List<string>();
            }
            var ex = new List<string>();
            if(string.IsNullOrEmpty(worksheet.Cells[row, 2].Text))
            {
                ex.Add($"In row ( {row - 2} ) field (student's First Name) : First Name is required");
            }
            if(string.IsNullOrEmpty(worksheet.Cells[row, 3].Text))
            {
                ex.Add($"In row ( {row - 2} ) field (student's Last Name) : Last Name is required");
            }
            if (string.IsNullOrEmpty(worksheet.Cells[row, 4].Text))
            {
                ex.Add($"In row ( {row - 2} ) field (student's Birthday) : Birthday is required");
            }
            if (string.IsNullOrEmpty(worksheet.Cells[row, 5].Text))
            {
                ex.Add($"In row ( {row - 2} ) field (student's Nationality) : Nationality is required");
            }
            if (string.IsNullOrEmpty(worksheet.Cells[row, 6].Text))
            {
                ex.Add($"In row ( {row - 2} ) field (student's Gender) : Gender is required");
            }
            if (string.IsNullOrEmpty(worksheet.Cells[row, 12].Text))
            {
                ex.Add($"In row ( {row - 2} ) field (parent's First Name) : First Name is required");
            }
            if (string.IsNullOrEmpty(worksheet.Cells[row, 14].Text))
            {
                ex.Add($"In row ( {row - 2} ) field (parent's Email) : Email is required");
            }else if (!regex.IsMatch(worksheet.Cells[row, 14].Text.Trim().Replace(" ", "")))
            {
                ex.Add($"In row ( {row - 2} ) field (parent's Email) : Email format is invalid");
            }
            return ex;
        }
        private bool IsRowValid(ExcelWorksheet worksheet, int row)
        {
            return !string.IsNullOrEmpty(worksheet.Cells[row, 2].Text) &&
                   !string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) &&
                   !string.IsNullOrEmpty(worksheet.Cells[row, 4].Text) &&
                   !string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) &&
                   !string.IsNullOrEmpty(worksheet.Cells[row, 6].Text) &&
                   !string.IsNullOrEmpty(worksheet.Cells[row, 12].Text) &&
                   !string.IsNullOrEmpty(worksheet.Cells[row, 14].Text);
        }

        private async Task<ApplicationUser> GetUserAsync(string email, ExcelWorksheet worksheet, int row)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    FirstName = worksheet.Cells[row, 12].Text,
                    LastName = worksheet.Cells[row, 13].Text,
                    Email = email,
                    Birthday = string.IsNullOrEmpty(worksheet.Cells[row, 15].Text) ? (DateTime?)null : DateTime.Parse(worksheet.Cells[row, 15].Text),
                    Birthplace = worksheet.Cells[row, 16].Text,
                    Nationality = worksheet.Cells[row, 17].Text,
                    PhoneNumber = worksheet.Cells[row, 18].Text,
                    EmergencyPhoneNumber = worksheet.Cells[row, 19].Text,
                    Gender = worksheet.Cells[row, 20].Text.Trim(),
                    City = worksheet.Cells[row, 21].Text,
                    Street = worksheet.Cells[row, 22].Text,
                    StreetNr = worksheet.Cells[row, 23].Text,
                    ZipCode = worksheet.Cells[row, 24].Text,
                    Job = worksheet.Cells[row, 25].Text,
                    Graduation = worksheet.Cells[row, 26].Text,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    NormalizedUserName = email.ToUpper(),
                    NormalizedEmail = email.ToUpper(),
                    EmailConfirmed = true,
                    ActiveUser = true
                };

                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var pass = "FekraSchule.2024";
                        var result = await _userManager.CreateAsync(user, pass);
                        if (!result.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                        }

                        var roleResult = await _userManager.AddToRoleAsync(user, DefaultRole.Parent);
                        if (!roleResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            throw new Exception(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        }

                        await transaction.CommitAsync();
                        await _emailSender.SendConfirmationEmailFromExcel(user, pass);
                    }
                    catch(Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(HandleLogFile.handleErrLogFile(User, "ExcelMigrationController", ex.Message));
                        throw;
                    }
                }
            }

            return user;
        }
        
        private Student CreateStudent(ExcelWorksheet worksheet, int row, string parentId)
        {
            return new Student
            {
                FirstName = worksheet.Cells[row, 2].Text.Trim(),
                LastName = worksheet.Cells[row, 3].Text.Trim(),
                Birthday = DateTime.Parse(worksheet.Cells[row, 4].Text.Trim()),
                Nationality = worksheet.Cells[row, 5].Text.Trim(),
                Gender = worksheet.Cells[row, 6].Text.Trim(),
                City = worksheet.Cells[row, 7].Text.Trim(),
                Street = worksheet.Cells[row, 8].Text.Trim(),
                StreetNr = worksheet.Cells[row, 9].Text.Trim(),
                ZipCode = worksheet.Cells[row, 10].Text.Trim(),
                Note = worksheet.Cells[row, 11].Text.Trim() ?? "",
                ActiveStudent= false,
                ParentID = parentId
            };
        }






    }
}



