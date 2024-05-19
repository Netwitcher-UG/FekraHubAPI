using System.Collections.Generic;
namespace FekraHubAPI.Constract
{
    public static class Permissions
    {
        public static List<string> GeneratePermissionsFromModule(string  module) {
            return new List<string>
            {
                $"Permissions.{module}.View",
                $"Permissions.{module}.Create",
                $"Permissions.{module}.Edit",
                $"Permissions.{module}.Delete",
            };
        }
    }
}
