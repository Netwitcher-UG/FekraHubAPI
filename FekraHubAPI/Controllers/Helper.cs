namespace FekraHubAPI.Controllers
{
    public class Helper
    {
        public const string Permission = "Permission";
        public enum PermissionModuleNameAdmin {
            Create_User,
            View_User,

        }

        public  enum AllRolesE
        {
            Admin,
            Secretariat,
            Parent,
            Teacher,
        }
    }
}
