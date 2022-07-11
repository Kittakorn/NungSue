namespace NungSue.Areas.Admin.ViewModels.Role
{
    public class RoleViewModel
    {
        public Guid RoleId { get; set; }
        public string Name { get; set; }
        public int UserCount { get; set; }
        public string PermissionCount { get; set; }
    }
}