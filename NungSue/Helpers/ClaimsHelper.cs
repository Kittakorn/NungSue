using NungSue.Areas.Admin.ViewModels.Role;
using NungSue.Models;
using System.Reflection;

namespace NungSue.Helpers
{
    public static class ClaimsHelper
    {
        public static void GetPermissions(this List<RolePermissionViewModel> allPermissions, Type policy)
        {
            var fields = policy.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields)
            {
                var permission = field.GetValue(null)! as PermissionField;
                allPermissions.Add(new RolePermissionViewModel { Value = permission!.Value, Description = permission!.Description });
            }
        }
    }
}