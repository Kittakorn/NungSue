using NungSue.Entities;
using System.ComponentModel.DataAnnotations;

namespace NungSue.ViewModels
{
    public class RegisterViewModel : IValidatableObject
    {
        [MaxLength(50)]
        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public string Email { get; set; }

        [MaxLength(50)]
        [MinLength(6, ErrorMessage = "ความยาว 6 - 50 ตัวอักษร")]
        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public string Password { get; set; }

        [MaxLength(50)]
        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        [Compare("Password", ErrorMessage = "รหัสผ่านไม่ตรงกัน กรุณาตรวจสอบ")]
        public string ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var context = validationContext.GetService<BookStoreContext>();
            var results = new List<ValidationResult>();

            if (context.Customers.Any(x => x.Email == Email))
                results.Add(new ValidationResult("อีเมลนี้ไม่สามรถใช้งานได้", new[] { nameof(Email) }));

            return results;
        }

    }
}