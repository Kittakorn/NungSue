using NungSue.Models;

namespace NungSue.Constants
{
    public static class Permissions
    {
        public static class Role
        {
            public static readonly PermissionField View = new("Admin.Role.View", "ดูรายการตำแหน่งงาน");
            public static readonly PermissionField Create = new("Admin.Role.Create", "เพิ่มข้อมูลตำแหน่ง");
            public static readonly PermissionField Edit = new("Admin.Role.Edit", "แก้ไขข้อมูลตำแหน่ง");
        }

        public static class User
        {
            public static readonly PermissionField View = new("Admin.User.View", "ดูรายชื่อพนักงงาน");
            public static readonly PermissionField Create = new("Admin.User.Create", "เพิ่มข้อมูลพนักงาน");
            public static readonly PermissionField Edit = new("Admin.User.Edit", "แก้ไขข้อมูลพนักงาน");
            public static readonly PermissionField Detail = new("Admin.User.Detail", "ดูรายละเอียดพนักงาน");
        }
    }
}