using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
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
        private readonly ILogger<ExcelMigrationController> _logger;
        private readonly EmailSender.IEmailSender _emailSender;
        private readonly ApplicationDbContext _dbContext ;
        public ExcelMigrationController(IRepository<Student> studentRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, 
            ILogger<ExcelMigrationController> logger, EmailSender.IEmailSender emailSender, ApplicationDbContext dbContext
             )
        {
            _studentRepository = studentRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
            _dbContext = dbContext;
        }
        [HttpGet("download-excelFile")]
        public IActionResult DownloadExcel()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "students.xlsx");

            if (!System.IO.File.Exists(filePath))
                return NotFound("file not found");

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "students.xlsx";

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, fileName);
        }
        
        //[Authorize(Policy = "ManageExcelMigration")]
        [HttpPost("UploadData")]
        public async Task<IActionResult> UploadData([Required] IFormFile file)
        {
            var createdUsers = new List<ApplicationUser>();          
            var notifications = new List<(ApplicationUser user, string pass)>(); 

            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("File is not selected or empty");

                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var count = 0;

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];
                var lastRow = worksheet.Dimension?.End.Row ?? 0;
                for (int row = 3; row <= lastRow; row++)
                {
                    var ex = ExcelExceptions(worksheet, row, emailRegex);
                    if (!string.IsNullOrEmpty(ex))
                        return BadRequest(ex);
                }

                await using var tx = await _dbContext.Database.BeginTransactionAsync();

                for (int row = 3; row <= lastRow; row++)
                {
                    if (IsRowCompletelyEmpty(worksheet, row)) continue;

                    var email = worksheet.Cells[row, 14].Text.Trim().Replace(" ", "");
                    var (user, created, pass) = await GetOrCreateUserAsync(email, worksheet, row);

                    if (created)
                    {
                        createdUsers.Add(user);                    
                        notifications.Add((user, pass!));         
                    }

                    var student = CreateStudent(worksheet, row, user.Id);
                    _dbContext.Students.Add(student);
                    count++;
                }

                await _dbContext.SaveChangesAsync();
                await tx.CommitAsync();

                foreach (var (user, pass) in notifications)
                {
                    try
                    {
                        await _emailSender.SendConfirmationEmailFromExcel(user, pass);
                    }
                    catch (Exception mailEx)
                    {
                        _logger.LogError(HandleLogFile.handleErrLogFile(User, "ExcelMigrationController", $"Email failed for {user.Email}: {mailEx.Message}"));
                    }
                }

                return Ok($"{count} students have been added");
            }
            catch (Exception ex)
            {
                foreach (var u in createdUsers)
                {
                    try { await _userManager.DeleteAsync(u); }
                    catch (Exception delEx)
                    {
                        _logger.LogError(HandleLogFile.handleErrLogFile(User, "ExcelMigrationController", $"Failed to rollback user {u.Email}: {delEx.Message}"));
                    }
                }

                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ExcelMigrationController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        private static string T(ExcelWorksheet ws, int r, int c)
            => ws.Cells[r, c].Text?.Trim();

        private static readonly int[] RequiredCols = { 2, 3, 4, 5, 6, 12, 14 };

        


        private bool IsRowCompletelyEmpty(ExcelWorksheet worksheet, int row)
        {
            foreach (var c in RequiredCols)
                if (!string.IsNullOrWhiteSpace(T(worksheet, row, c)))
                    return false;
            return true;
        }

        


        private string ExcelExceptions(ExcelWorksheet worksheet, int row, Regex regex)
        {
            if (IsRowCompletelyEmpty(worksheet, row))
                return "";

            if (string.IsNullOrWhiteSpace(T(worksheet, row, 2)))
                return $"In row ( {row - 2} ) field (student's First Name) : First Name is required";

            if (string.IsNullOrWhiteSpace(T(worksheet, row, 3)))
                return $"In row ( {row - 2} ) field (student's Last Name) : Last Name is required";

            if (string.IsNullOrWhiteSpace(T(worksheet, row, 4)))
                return $"In row ( {row - 2} ) field (student's Birthday) : Birthday is required";

            if (!DateTime.TryParse(T(worksheet, row, 4), out _))
                return $"In row ( {row - 2} ) field (student's Birthday) : Birthday format is invalid";

            if (string.IsNullOrWhiteSpace(T(worksheet, row, 5)))
                return $"In row ( {row - 2} ) field (student's Nationality) : Nationality is required";

            if (string.IsNullOrWhiteSpace(T(worksheet, row, 6)))
                return $"In row ( {row - 2} ) field (student's Gender) : Gender is required";

            if (string.IsNullOrWhiteSpace(T(worksheet, row, 12)))
                return $"In row ( {row - 2} ) field (parent's First Name) : First Name is required";

            var email = T(worksheet, row, 14);
            if (string.IsNullOrWhiteSpace(email))
                return $"In row ( {row - 2} ) field (parent's Email) : Email is required";

            if (!regex.IsMatch(email.Replace(" ", "")))
                return $"In row ( {row - 2} ) field (parent's Email) : Email format is invalid";


            return "";
        }


        private async Task<(ApplicationUser user, bool created, string? password)> GetOrCreateUserAsync(
    string email, ExcelWorksheet worksheet, int row)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
                return (user, false, null);

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

            var pass = "FekraSchule.2024";
            var create = await _userManager.CreateAsync(user, pass);
            if (!create.Succeeded)
                throw new Exception(string.Join(", ", create.Errors.Select(e => e.Description)));

            var role = await _userManager.AddToRoleAsync(user, DefaultRole.Parent);
            if (!role.Succeeded)
                throw new Exception(string.Join(", ", role.Errors.Select(e => e.Description)));

            return (user, true, pass);
        }

        private Student CreateStudent(ExcelWorksheet worksheet, int row, string parentId)
        {
            var bdayText = T(worksheet, row, 4);
            var birthday = DateTime.Parse(bdayText); 

            return new Student
            {
                FirstName = T(worksheet, row, 2),
                LastName = T(worksheet, row, 3),
                Birthday = birthday,
                Nationality = T(worksheet, row, 5),
                Gender = T(worksheet, row, 6),
                City = T(worksheet, row, 7),
                Street = T(worksheet, row, 8),
                StreetNr = T(worksheet, row, 9),
                ZipCode = T(worksheet, row, 10),
                Note = (worksheet.Cells[row, 11].Text ?? string.Empty).Trim(),
                ActiveStudent = false,
                ParentID = parentId
            };
        }







    }
}



