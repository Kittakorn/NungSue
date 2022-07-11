using NungSue.Areas.Admin.ViewModels.Book;
using NungSue.Areas.Admin.ViewModels.User;
using System.ComponentModel.DataAnnotations;

namespace NungSue.Helpers
{
    public class CustomValidation
    {
        public class RequiredBookImageIfIdNull : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var model = validationContext.ObjectInstance as BookCreateViewModel;
                if (model.BookId == Guid.Empty && model.BookImage == null)
                    return new ValidationResult(ErrorMessage);
                return ValidationResult.Success;
            }
        }

        public class RequiredUserImageIfIdNull : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var model = validationContext.ObjectInstance as UserCreateViewModel;
                if (model.UserId == Guid.Empty && model.ProfileImage == null)
                    return new ValidationResult(ErrorMessage);
                return ValidationResult.Success;
            }
        }
    }
}