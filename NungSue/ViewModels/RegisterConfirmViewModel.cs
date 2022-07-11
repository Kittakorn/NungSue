using NungSue.Entities;
using System.ComponentModel.DataAnnotations;

namespace NungSue.ViewModels;

public class RegisterConfirmViewModel : IValidatableObject
{
    [MaxLength(50)]
    [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
    public string Email { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var context = validationContext.GetService<NungSueContext>();
        var results = new List<ValidationResult>();

        if (context.Customers.Any(x => x.Email == Email))
            results.Add(new ValidationResult("อีเมลนี้ไม่สามารถใช้งานได้", new[] { nameof(Email) }));

        return results;
    }
}
