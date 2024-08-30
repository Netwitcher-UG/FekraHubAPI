using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.MapModels.SchoolInfo
{
    public class Map_SchoolInfo_Basic 
    {
        
        public string SchoolName { get; set; }
        public string SchoolOwner { get; set; }
        public IFormFile Logo { get; set; }
        
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        

       
    }
}
