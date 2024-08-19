using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace FekraHubAPI.Constract
{
    public sealed class PermissionsEnum
    {
        public const string Permission = "Permission";


        public enum AllPermissions
        {
            GetUsers,
            AddUsers,
            GetStudentsCourse,
            UpdateStudentsCourse,   // later
            GetTeachersCourse,//
            UpdateTeachersCourse,//
            GetContracts,
            ManageEvents,  
            GetEvents,
            GetStudentsAttendance,
            UpdateStudentsAttendance, 
            GetTeachersAttendance,
            UpdateTeachersAttendance,
            InsertUpdateStudentsReports, 
            GetStudentsReports,
            ApproveReports,
            ManagePayrolls,
            ManageBooks,
            DeleteSchoolData, // 
            ResetData , //
            //============
            ManagePermissions,
            ManageCourseSchedule,
            ManageEventTypes,
            ManageFile,
            DeleteFile,
            ManageAttendanceStatus,
            AddStudentAttendance,
            GetCourse,
            AddCourse,
            putCourse,
            DeleteCourse,
            ManageStudentsToCourses,
            ManageLocations,
            ManageRoom,
            ManageExcelMigration,
            GetEmployee,
            GetSecretary,
            GetTeacher,
            GetParent,
            UpdateUser,
            DeleteUser,
            ResetPasswordUser,
            ManageWorkContract,
            ExportReport,
            ManageSchoolInfo,
            ManageChildren,
            ShowParent,
            ManageInvoice,
            ShowParentInfo


        }
        


        public static bool CheckPermissionExist(string permission)
        {
            return Enum.IsDefined(typeof(AllPermissions), permission);
        }
    }
}
