using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_WorkContract : IMapFrom<WorkContract>


    {
        public byte[] File { get; set; }

        public string? TeacherID { get; set; }

    }
}
