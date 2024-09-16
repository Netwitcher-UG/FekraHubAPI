using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using FekraHubAPI.Seeds;
using FekraHubAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using FekraHubAPI.EmailSender;
using System.Security.Claims;
using FekraHubAPI.Repositories.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Metadata;
using System.IO;
using FekraHubAPI.MapModels.Users;
using FekraHubAPI.MapModels.Response;
using Microsoft.AspNetCore.Identity.UI.Services;
using FekraHubAPI.MapModels;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
using FekraHubAPI.Constract;


namespace FekraHubAPI.Controllers.UsersController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersManagment : ControllerBase
    {
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
       // private  UserManager<ApplicationUser>  currentUser ;
        private readonly IRepository<ApplicationUser> _applicationUserRepository;
        private readonly ApplicationUsersServices _applicationUsersServices;
        private readonly EmailSender.IEmailSender _emailSender;
        private readonly ILogger<UsersManagment> _logger;
        public UsersManagment(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, IRepository<ApplicationUser> applicationUserRepository , ApplicationUsersServices applicationUsersServices  ,
        ApplicationDbContext db, EmailSender.IEmailSender emailSender, ILogger<UsersManagment> logger)
        {

            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _applicationUserRepository = applicationUserRepository;
            _applicationUsersServices = applicationUsersServices;
            _emailSender = emailSender;
            _logger = logger;

        }
        [Authorize(Policy = "GetUsers")]
        [HttpGet("PaginationParameters")]
        public async Task<IActionResult> PaginationParameters([FromQuery] PaginationParameters paginationParameters)
        {
            try
            {
                var users = await _applicationUserRepository.GetRelationAsQueryable(
                    selector: x => new
                    {
                        x.Id,
                        x.FirstName,
                        x.LastName,
                        x.Email,
                        x.PhoneNumber,
                        x.EmergencyPhoneNumber,
                        x.Birthday,
                        x.Birthplace,
                        x.City,
                        x.Street,
                        x.StreetNr,
                        x.ZipCode,
                        x.Job,
                        x.Gender,
                        x.Graduation,
                        x.Nationality
                    },
                    asNoTracking:true
                    );
                var p = await  _applicationUserRepository.GetPagedDataAsync(users , paginationParameters);
                return Ok(new
                {
                    p.CurrentPage,
                    p.PageSize,
                    p.TotalCount,
                    p.TotalPages,
                    p.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UsersManagment", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        [Authorize(Policy = "GetUsers")]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                var allUsers = await _db.ApplicationUser.ToListAsync();

                if (userRole != DefaultRole.Admin)
                {
                    allUsers = await _applicationUsersServices.GetAllNonAdminUsersAsync();
                }
                var data = allUsers.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.UserName,
                    x.FirstName,
                    x.LastName,
                    x.Email,
                    x.ImageUser,
                    x.Gender,
                    x.Job,
                    x.Birthday,
                    x.Birthplace,
                    x.Nationality,
                    x.City,
                    x.Street,
                    x.StreetNr,
                    x.ZipCode,
                    x.PhoneNumber,
                    x.EmergencyPhoneNumber,

                }).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UsersManagment", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        [Authorize(Policy = "GetEmployee")]
        [HttpGet("GetEmployee")]
        public async Task<IActionResult> GetEmployee(string? RoleName)
        {
            try
            {
                List<string> roleIds = new List<string>();
                if (RoleName != null) 
                {
                     roleIds = await _db.Roles.Where(x => x.Name == RoleName)
                        .Select(r => r.Id).ToListAsync();
                    if (!roleIds.Any())  
                    {
                        return BadRequest("RoleName not found");
                    }
                    if(!roleIds.Any(x=> x =="1" || x =="2" ||x == "4"))
                    {
                        return BadRequest("RoleName not an employee");
                    }
                }
                else
                {
                    roleIds = new List<string> { "1", "2", "4" };
                }
                var userIds = await _db.UserRoles
                                .Where(ur => roleIds.Contains(ur.RoleId))
                                .Select(ur => ur.UserId)
                                .ToListAsync();

                var users = await _db.ApplicationUser
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();

                var userRoles = await _db.UserRoles
                    .Where(ur => userIds.Contains(ur.UserId))
                    .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new
                    {
                        ur.UserId,
                        r.Name
                    })
                    .ToListAsync();

                var result = users.Select(user => new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.ImageUser,
                    user.Gender,
                    user.Job,
                    user.Birthday,
                    user.Birthplace,
                    user.Nationality,
                    user.City,
                    user.Street,
                    user.StreetNr,
                    user.ZipCode,
                    user.PhoneNumber,
                    user.EmergencyPhoneNumber,

                    Roles = RoleName != null ? RoleName : string.Join(", ", userRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.Name))
                }).ToList();

                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UsersManagment", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        
        [Authorize(Policy = "GetTeacher")]
        [HttpGet("GetTeacher")]
        public async Task<IActionResult> GetTeacher()
        {
            try
            {
                var role = "4";
                var userIdsInRoles = await _db.UserRoles
                                    .Where(x => x.RoleId == role)
                                    .Select(x => x.UserId)
                                    .ToListAsync();
                var usersInRoles = await _db.ApplicationUser
                                    .Where(x => userIdsInRoles.Contains(x.Id))
                                    .ToListAsync();

                var data = usersInRoles.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.UserName,
                    x.FirstName,
                    x.LastName,
                    x.Email,
                    x.ImageUser,
                    x.Gender,
                    x.Job,
                    x.Birthday,
                    x.Birthplace,
                    x.Nationality,
                    x.City,
                    x.Street,
                    x.StreetNr,
                    x.ZipCode,
                    x.PhoneNumber,
                    x.EmergencyPhoneNumber,

                }).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UsersManagment", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "GetTeacher")]
        [HttpGet("TeacherProfile")]
        public async Task<IActionResult> GetTeacherProfil(string id)
        {
            var teacher = await _db.ApplicationUser.Where(x => x.Id == id).AsNoTracking().Select(s=> new
            {
                s.Id,
                s.FirstName,
                s.LastName,
                s.Email,
                s.Gender,
                s.Job,
                s.Birthday,
                s.Birthplace,
                s.Nationality,
                s.City,
                s.Street,
                s.StreetNr,
                s.ZipCode,
                s.PhoneNumber,
                s.EmergencyPhoneNumber,
                s.Graduation
            }).SingleOrDefaultAsync();
            var courses = await _db.Courses
                .Include(t => t.Teacher)
                .Where(x => x.Teacher.Select(z => z.Id).Contains(teacher.Id))
                .Include(x=>x.Room).ThenInclude(x=>x.Location).Include(x=>x.Student)
                .AsNoTracking()
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Capacity,
                    c.Lessons,
                    c.Price,
                    c.StartDate,
                    c.EndDate,
                    StudentCount = c.Student.Count,
                    AnotherTeachers = c.Teacher.Where(x=>x.Id != id).Select(z => new 
                    {
                        z.Id,
                        z.FirstName,
                        z.LastName,
                        z.Email
                    }), 
                    Room = new
                    {
                        c.Room.Id,
                        c.Room.Name
                    },
                    Location = new
                    {
                        c.Room.Location.Id,
                        c.Room.Location.Name,
                        c.Room.Location.Street,
                        c.Room.Location.StreetNr,
                        c.Room.Location.City,
                        c.Room.Location.ZipCode,

                    }

                })
                .ToListAsync();
            return Ok(new { teacher,courses });
        }
        [Authorize(Policy = "GetSecretary")]
        [HttpGet("GetSecretary")]
        public async Task<IActionResult> GetSecretary()
        {
            try
            {
                var role = "2";
                var userIdsInRoles = await _db.UserRoles
                                    .Where(x => x.RoleId == role)
                                    .Select(x => x.UserId)
                                    .ToListAsync();
                var usersInRoles = await _db.ApplicationUser
                                    .Where(x => userIdsInRoles.Contains(x.Id))
                                    .ToListAsync();

                var data = usersInRoles.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.UserName,
                    x.FirstName,
                    x.LastName,
                    x.Email,
                    x.ImageUser,
                    x.Gender,
                    x.Job,
                    x.Birthday,
                    x.Birthplace,
                    x.Nationality,
                    x.City,
                    x.Street,
                    x.StreetNr,
                    x.ZipCode,
                    x.PhoneNumber,
                    x.EmergencyPhoneNumber,

                }).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UsersManagment", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        [Authorize(Policy = "GetParent")]
        [HttpGet("GetPerent")]
        public async Task<IActionResult> GetPerent()
        {
            try
            {
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                var allUsers = await _db.ApplicationUser.ToListAsync();


                if (userRole != DefaultRole.Admin)
                {
                    allUsers = await _applicationUsersServices.GetAllNonAdminUsersAsync();
                }

                allUsers = allUsers.Where(x => _userManager.IsInRoleAsync(x, "Parent").Result)
                    .ToList();

                var data = allUsers.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.UserName,
                    x.FirstName,
                    x.LastName,
                    x.Email,
                    x.ImageUser,
                    x.Gender,
                    x.Job,
                    x.Birthday,
                    x.Birthplace,
                    x.Nationality,
                    x.City,
                    x.Street,
                    x.StreetNr,
                    x.ZipCode,
                    x.PhoneNumber,
                    x.EmergencyPhoneNumber,

                }).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UsersManagment", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        
        [Authorize(Policy = "GetUsers")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                var userRole = User.FindFirstValue(ClaimTypes.Role);  // current role

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"user not exists!");
                }

                var isAdmin = await _userManager.IsInRoleAsync(user, DefaultRole.Admin);

                if (userRole != DefaultRole.Admin && isAdmin)
                {
                    return BadRequest("Cant Access This User");
                }
                var data = new
                {
                    user.Id,
                    user.Name,
                    user.UserName,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.ImageUser,
                    user.Gender,
                    user.Job,
                    user.Birthday,
                    user.Birthplace,
                    user.Nationality,
                    user.City,
                    user.Street,
                    user.StreetNr,
                    user.ZipCode,
                    user.PhoneNumber,
                    user.EmergencyPhoneNumber,

                };
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UsersManagment", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "AddUsers")]
        [HttpPost]
        public async Task<IActionResult> AddUser([FromForm] Map_Account user)
        {
            try
            {
                var img = "";
                if (user.ImageUser != null && user.ImageUser.Length != 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await user.ImageUser.CopyToAsync(memoryStream);
                        var imageBytes = memoryStream.ToArray();
                        img = Convert.ToBase64String(imageBytes);
                    }
                }
                var IsEmailExists = await _userManager.FindByEmailAsync(user.Email);
                if (IsEmailExists != null)
                {
                    return BadRequest($"Email {user.Email} is already token.");
                }

                using (IDbContextTransaction transaction = _db.Database.BeginTransaction())
                {

                    string RoleParent = DefaultRole.Parent;
                    if (ModelState.IsValid)
                    {
                        var normalizedEmail = user.Email.ToUpperInvariant();
                        var normalizedUserName = user.Email.ToUpperInvariant();
                        ApplicationUser appUser = new()
                        {
                            UserName = user.Email,
                            NormalizedUserName = normalizedUserName,
                            Email = user.Email,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            ImageUser = img,
                            NormalizedEmail = normalizedEmail,
                            SecurityStamp = Guid.NewGuid().ToString("D"),
                            PhoneNumber = user.PhoneNumber,
                            Gender = user.Gender,
                            EmergencyPhoneNumber = user.EmergencyPhoneNumber,
                            Birthday = user.Birthday,
                            Birthplace = user.Birthplace,
                            Nationality = user.Nationality,
                            Street = user.Street,
                            StreetNr = user.StreetNr,
                            ZipCode = user.ZipCode,
                            City = user.City,
                            Job = user.Job,
                            Graduation = user.Graduation
                        };

                        IdentityResult result = await _userManager.CreateAsync(appUser, user.Password);



                        if (result.Succeeded)
                        {

                            if (DefaultRole.checkRole(user.Role))
                            {
                                _userManager.AddToRoleAsync(appUser, user.Role).Wait();
                                transaction.Commit();
                                _ = Task.Run(() => _emailSender.SendConfirmationEmailWithPassword(appUser, user.Password));
                                return Ok();
                            }
                            else
                            {
                                transaction.Rollback();
                                return BadRequest("Role Not Found");
                            }


                        }
                        else
                        {
                            return BadRequest(result.Errors.Select(x => x.Description).FirstOrDefault());
                        }
                    }
                    transaction.Rollback();
                    return BadRequest(ModelState);


                }
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UsersManagment", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "UpdateUser")]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromForm] Map_Account accountUpdate)
        {
            try
            {
                var account = await _db.ApplicationUser.FindAsync(id);
                if (account == null)
                {
                    return NotFound($" account id {id} not exists !");
                }

                string folderFile = "images/users/";
                if (accountUpdate.ImageUser != null)
                {
                    if (account.ImageUser != accountUpdate.ImageUser.FileName)
                    {
                        string folder = Guid.NewGuid().ToString() + "_" + accountUpdate.ImageUser.FileName;
                        string image = folderFile + folder;

                        if (!System.IO.Directory.Exists(folderFile))
                        {
                            System.IO.Directory.Delete(folderFile);

                        }
                        string serverFolder = Path.Combine(folderFile, folder);
                        using var stream = new FileStream(serverFolder, FileMode.Create);
                        accountUpdate.ImageUser.CopyTo(stream);
                        account.ImageUser = image;
                        //image = user.imageUser.ToString();
                    }

                }



                // await _applicationUserRepository.Update(account);

                var normalizedEmail = accountUpdate.Email.Normalize().ToLower();
                var normalizedUserName = accountUpdate.Email.Normalize().ToLower();

                account.UserName = accountUpdate.Email;
                account.Email = accountUpdate.Email;
                account.NormalizedUserName = normalizedUserName;
                account.NormalizedEmail = normalizedEmail;

                account.FirstName = accountUpdate.FirstName;
                account.LastName = accountUpdate.LastName;
                account.SecurityStamp = Guid.NewGuid().ToString("D");
                account.PhoneNumber = accountUpdate.PhoneNumber;
                account.Gender = accountUpdate.Gender;
                account.EmergencyPhoneNumber = accountUpdate.EmergencyPhoneNumber;
                account.Birthday = accountUpdate.Birthday;
                account.Birthplace = accountUpdate.Birthplace;
                account.Nationality = accountUpdate.Nationality;
                account.Street = accountUpdate.Street;
                account.StreetNr = accountUpdate.Street;
                account.ZipCode = accountUpdate.ZipCode;
                account.City = accountUpdate.City;
                account.Job = accountUpdate.Job;
                account.Graduation = accountUpdate.Graduation;

                _db.SaveChanges();
                return Ok(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UsersManagment", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        [Authorize(Policy = "DeleteUser")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser.Id == id)
                {
                    return BadRequest("Cant Delete This User");
                }
                var deleteUser = await _db.ApplicationUser.SingleOrDefaultAsync(x => x.Id == id);
                if (deleteUser == null)
                {
                    return NotFound($"user id not exists !");
                }
                _db.ApplicationUser.Remove(deleteUser);
                await _db.SaveChangesAsync();
                return Ok("User Deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UsersManagment", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "ResetPasswordUser")]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ResetPasswordUser([Required] string id , [Required] string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetPassResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
                    if (resetPassResult.Succeeded)
                    {
                        return Ok();

                    }
                    return StatusCode(StatusCodes.Status400BadRequest,
                       new Response { Status = "Error", Message = resetPassResult.ToString() });
                    //var forgotPasswordLink = Url.Action(nameof(ResetPassword), "Authentication", new { token, email = user.Email }, Request.Scheme);
                    /*var message = new Message(new string[] { user.Email! }, "Forgot Password Link", forgotPasswordLink!);
                    _emailService.SendEmail(message);

                    return StatusCode(StatusCodes.Status2000K,
                    new Response { Status = "Success", Message = $"User created & Email Sent to {user.Email} SuccessFully" });
                    */
                    //return Ok(token);

                }
                return StatusCode(StatusCodes.Status400BadRequest,
                       new Response { Status = "Error", Message = $"Could not send link to email , please try again." });

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UsersManagment", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize]
        [HttpGet("UserProfile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var userID = _applicationUserRepository.GetUserIDFromToken(User);
                var userRole = await _db.UserRoles.Where(x => x.UserId == userID).Select(x => x.RoleId).SingleOrDefaultAsync();
                if (userRole == null)
                {
                    return BadRequest("Role not exist");
                }
                if (userRole == "4")
                {
                    var user = await _applicationUserRepository.GetRelationSingle(
                    where: x => x.Id == userID,
                    include: x => x.Include(z => z.Course).ThenInclude(z => z.Room).ThenInclude(z => z.Location).Include(z => z.Course).ThenInclude(c => c.CourseSchedule),
                    selector: s => new
                    {
                        s.Id,
                        s.FirstName,
                        s.LastName,
                        s.Email,
                        s.Gender,
                        s.Job,
                        s.Birthday,
                        s.Birthplace,
                        s.Nationality,
                        s.City,
                        s.Street,
                        s.StreetNr,
                        s.ZipCode,
                        s.PhoneNumber,
                        s.EmergencyPhoneNumber,
                        s.Graduation,
                        s.ImageUser,
                        course = s.Course.Select(x => new
                        {
                            x.Id,
                            x.Name,
                            x.Capacity,
                            x.StartDate,
                            x.EndDate,
                            schedule = x.CourseSchedule.Select(z => new
                            {
                                z.Id,
                                z.DayOfWeek,
                                z.StartTime,
                                z.EndTime
                            }),
                            Room = new
                            {
                                x.Room.Id,
                                x.Room.Name,
                                Location = new
                                {
                                    x.Room.Location.Id,
                                    x.Room.Location.Name,
                                    x.Room.Location.Street,
                                    x.Room.Location.StreetNr,
                                    x.Room.Location.ZipCode,
                                    x.Room.Location.City
                                }
                            }
                        }),
                        TeacherAttendance = s.TeacherAttendance.Select(a => new
                        {
                            a.Id,
                            a.date,
                            a.AttendanceStatus.Title
                        })
                    },
                    returnType: QueryReturnType.SingleOrDefault,
                    asNoTracking: true
                    );
                    return Ok(user);
                }
                else
                {
                    var user = await _applicationUserRepository.GetRelationSingle(
                       where: x => x.Id == userID,
                       selector: s => new
                       {
                           s.Id,
                           s.FirstName,
                           s.LastName,
                           s.Email,
                           s.Gender,
                           s.Job,
                           s.Birthday,
                           s.Birthplace,
                           s.Nationality,
                           s.City,
                           s.Street,
                           s.StreetNr,
                           s.ZipCode,
                           s.PhoneNumber,
                           s.EmergencyPhoneNumber,
                           s.Graduation,
                           s.ImageUser,
                       },
                    returnType: QueryReturnType.SingleOrDefault,
                    asNoTracking: true
                    );
                    return Ok(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UsersManagment", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        public class UserDTO
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Gender { get; set; }
            public string Job { get; set; }
            public string PhoneNumber { get; set; }
            public string EmergencyPhoneNumber { get; set; }
            public string ZipCode { get; set; }
            public string Street { get; set; }
            public string City { get; set; }
            public string StreetNr { get; set; }
            public string Nationality { get; set; }
            public string Graduation { get; set; }
            public IFormFile? Image { get; set; }
        }
        [Authorize]
        [HttpPut("UserProfile")]
        public async Task<IActionResult> UpdateUserProfile([FromForm] UserDTO userData)
        {
            try
            {
                var userID = _applicationUserRepository.GetUserIDFromToken(User);
                var user = await _db.ApplicationUser.FindAsync(userID);
                var img = "";
                if (userData.Image != null && userData.Image.Length != 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await userData.Image.CopyToAsync(memoryStream);
                        var imageBytes = memoryStream.ToArray();
                        img = Convert.ToBase64String(imageBytes);
                    }
                }
                user.FirstName = userData.FirstName;
                user.LastName = userData.LastName;
                user.Email = userData.Email;
                user.PhoneNumber = userData.PhoneNumber;
                user.EmergencyPhoneNumber = userData.EmergencyPhoneNumber;
                user.City = userData.City;
                user.StreetNr = userData.StreetNr;
                user.Nationality = userData.Nationality;
                user.Street = userData.Street;
                user.Gender = userData.Gender;
                user.ZipCode = userData.ZipCode;
                user.Graduation = userData.Graduation;
                user.Job = userData.Job;
                user.ImageUser = img;
                await _db.SaveChangesAsync();
                return Ok(new
                {
                    user.FirstName ,
                    user.LastName ,
                    user.Email ,
                    user.PhoneNumber ,
                    user.EmergencyPhoneNumber ,
                    user.City ,
                    user.StreetNr ,
                    user.Nationality,
                    user.Street,
                    user.Gender ,
                    user.ZipCode ,
                    user.Graduation,
                    user.Job,
                    user.ImageUser,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UsersManagment", ex.Message));
                return BadRequest(ex.Message);
            }
        }
    }
}
