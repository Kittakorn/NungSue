using FluentValidation;
using NungSue.ViewModels;

namespace NungSue.Validators;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordViewModel>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.OldPassword)
           .NotNull().WithMessage("กรุณากรอกข้อมูล")
           .Length(6, 20).WithMessage("ความยาว 6 - 20 ตัวอักษร");

        RuleFor(x => x.NewPassword)
            .NotNull().WithMessage("กรุณากรอกข้อมูล")
            .Length(6, 20).WithMessage("ความยาว 6 - 20 ตัวอักษร");

        RuleFor(x => x.ConfirmPassword)
            .NotNull().WithMessage("กรุณากรอกข้อมูล")
            .Equal(x => x.NewPassword).WithMessage("ยืนยันรหัสผ่านไม่ถูกต้อง");
    }
}
