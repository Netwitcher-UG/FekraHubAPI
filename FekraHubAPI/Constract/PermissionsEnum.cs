using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace FekraHubAPI.Constract
{
    public sealed class PermissionsEnum
    {
        public const string Permission = "Permission";


        public enum AllPermissions
        {
            View_Users,
            Add_Users,
            View_Student_Placement,
            Edit_Student_Placement,
            View_Teacher_Placement,
            Edit_Teacher_Placement,
            View_Contracts,
            Manage_Contracts,
            View_Events,
            View_General_Attendance,
            Edit_General_Attendance,
            View_Teacher_Attendance,
            Edit_Teacher_Attendance,
            View_Reports,
            Approve_Reports,
            Add_Bills,
            View_Bills,
            View_Books,
            Delete_Book,
            Delete_School_Aata,
            Reset_Data


        }

        public enum PermissionModuleNameAdmin
        {
            View_Users,
            Add_Users,
            View_Student_Placement,
            Edit_Student_Placement,
            View_Teacher_Placement,
            Edit_Teacher_Placement,
            View_Contracts,
            Manage_Contracts,
            View_Events,
            View_General_Attendance,
            Edit_General_Attendance,
            View_Teacher_Attendance,
            Edit_Teacher_Attendance,
            View_Reports,
            Approve_Reports,
            Add_Bills,
            View_Bills,
            View_Books,
            Delete_Book,
            Delete_School_Aata,
            Reset_Data

        }


        public static bool CheckPermissionExist(string permission)
        {
            return Enum.IsDefined(typeof(AllPermissions), permission);
        }
    }
}
