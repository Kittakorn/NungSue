using System.ComponentModel.DataAnnotations;

namespace NungSue.ViewModels
{
    public class SignInViewModel
    {
        [MaxLength(50)]
        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public string Email { get; set; }

        [MaxLength(50)]
        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}