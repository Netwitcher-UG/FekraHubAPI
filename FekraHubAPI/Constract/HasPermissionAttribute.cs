using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace FekraHubAPI.Constract
{
    public sealed class HasPermissionAttribute  : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission ) : base(policy:permission) {
            /*permission.
            options.AddPolicy("Create_View", policy =>
        policy.RequireClaim("type", "Create_View"));*/
        }
    }
}
