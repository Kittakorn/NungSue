using NungSue.Entities;
using System.ComponentModel.DataAnnotations;
using static NungSue.Helpers.CustomValidation;

namespace NungSue.Areas.Admin.ViewModels.User
{
    public class UserCreateViewModel : IValidatableObject
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        [MaxLength(20)]
        [MinLength(6, ErrorMessage = "ความยาว 6 - 20 ตัวอักษร")]
        public string Username { get; set; }

        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(12)]
        [MinLength(12, ErrorMessage = "เบอร์โทรศัพท์ไม่ถูกต้อง")]
        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public string PhoneNumber { get; set; }

        [RequiredUserImageIfIdNull(ErrorMessage = "กรุณาเลือกรูปประจำตัว")]
        public IFormFile ProfileImage { get; set; }

        public string ProfileImageUrl { get; set; }

        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public List<Guid> RoleIds { get; set; }

        public bool IsActive { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var context = validationContext.GetService<NungSueContext>();
            var results = new List<ValidationResult>();

            if (context.Users.Any(x => x.Username == Username && x.UserId != UserId))
                results.Add(new ValidationResult("มีชื่อผู้ใช้งานนี้ในระบบแล้ว", new[] { nameof(Username) }));

            return results;
        }
    }
}