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
            UpdateStudentsCourse,
            GetTeachersCourse,
            UpdateTeachersCourse,
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
            DeleteSchoolData,
            ResetData

        }
        


        public static bool CheckPermissionExist(string permission)
        {
            return Enum.IsDefined(typeof(AllPermissions), permission);
        }
    }
}
