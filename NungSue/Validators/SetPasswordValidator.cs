using FluentValidation;
using NungSue.ViewModels;

namespace NungSue.Validators;

public class SetPasswordValidator : AbstractValidator<SetPasswordViewModel>
{
    public SetPasswordValidator()
    {
        RuleFor(x => x.Password)
            .NotNull().WithMessage("กรุณากรอกข้อมูล")
            .Length(6, 20).WithMessage("ความยาว 6 - 20 ตัวอักษร");

        RuleFor(x => x.ConfirmPassword)
            .NotNull().WithMessage("กรุณากรอกข้อมูล")
            .Equal(x => x.Password).WithMessage("ยืนยันรหัสผ่านไม่ถูกต้อง");
    }
}
