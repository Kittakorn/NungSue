using FluentValidation;
using NungSue.ViewModels;

namespace NungSue.Validators;

public class AddressCreateValidator : AbstractValidator<AddressCreateViewModel>
{
    public AddressCreateValidator()
    {
        RuleFor(x => x.FirstName)
            .NotNull().WithMessage("กรุณากรอกข้อมูล")
            .MaximumLength(50).WithMessage("ความยาวไม่เกิน 50 ตัวอักษร");

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("ความยาวไม่เกิน 50 ตัวอักษร");

        RuleFor(x => x.PhoneNumber)
            .NotNull().WithMessage("กรุณากรอกข้อมูล")
            .Length(12).WithMessage("เบอร์โทรศัพท์ไม่ถูกต้อง");

        RuleFor(x => x.Address)
            .NotNull().WithMessage("กรุณากรอกข้อมูล")
            .MaximumLength(250).WithMessage("ความยาวไม่เกิน 250 ตัวอักษร");

        RuleFor(x => x.SubDistrict)
            .NotNull().WithMessage("กรุณากรอกข้อมูล")
            .MaximumLength(250).WithMessage("ความยาวไม่เกิน 50 ตัวอักษร");

        RuleFor(x=>x.District)
            .NotNull().WithMessage("กรุณากรอกข้อมูล")
            .MaximumLength(250).WithMessage("ความยาวไม่เกิน 50 ตัวอักษร");

        RuleFor(x => x.Province)
            .NotNull().WithMessage("กรุณากรอกข้อมูล")
            .MaximumLength(250).WithMessage("ความยาวไม่เกิน 50 ตัวอักษร");

        RuleFor(x=>x.ZipCode)
            .NotNull().WithMessage("กรุณากรอกข้อมูล")
            .Length(5).WithMessage("รหัสไปรษณีย์ไม่ถูกต้อง");
    }
}