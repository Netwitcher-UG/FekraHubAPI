using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.Models.Courses
{
    public class mdl_Upload: IMapFrom<Upload>
    {

        public byte[] file { get; set; }

        public int? UploadTypeID { get; set; }
    

        public void Mapping(Profile profile)
        {
            profile.CreateMap<mdl_Upload, Upload>();
        }

    }
}
