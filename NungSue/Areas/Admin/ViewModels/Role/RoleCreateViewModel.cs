using NungSue.Entities;
using System.ComponentModel.DataAnnotations;

namespace NungSue.Areas.Admin.ViewModels.Role
{
    public class RoleCreateViewModel : IValidatableObject
    {
        public Guid RoleId { get; set; }

        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public string RoleName { get; set; }

        public List<RolePermissionViewModel> RolePermissions { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var context = validationContext.GetService<NungSueContext>();
            var results = new List<ValidationResult>();

            if (RoleId != Guid.Empty && context.Roles.Any(x => x.Name == RoleName && x.RoleId != RoleId))
                results.Add(new ValidationResult("ชื่อตำแหน่งนี้มีในระบบแล้ว กรุณาเลือกชื่ออื่น", new[] { nameof(RoleName) }));

            if (!RolePermissions.Any(x => x.Selected))
                results.Add(new ValidationResult("กรุณาเลือกอย่างน้อย 1 สิทธิ์", new[] { nameof(RolePermissions) }));

            return results;
        }
    }
}