using Microsoft.AspNetCore.Authorization;

namespace FekraHubAPI.Constract
{
    public sealed class HasPermissionAttribute  : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission ) : base(policy:permission) { 
        }
    }
}
