namespace NungSue.Areas.Admin.ViewModels.User
{
    public class UserViewModel
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string CreateDate { get; set; }
        public bool IsActive { get; set; }
        public string Roles { get; set; }
        public string LastLogin { get; set; }
    }
}