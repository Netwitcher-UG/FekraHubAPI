﻿using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_TeacherAttendance 
    {
        public DateTime Date { get; set; }
        public string TeacherID { get; set; }
        public int StatusID { get; set; }

        
    }
}
