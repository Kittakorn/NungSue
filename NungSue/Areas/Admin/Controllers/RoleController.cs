using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NungSue.Areas.Admin.ViewModels.Role;
using NungSue.Constants;
using NungSue.Entities;
using NungSue.Helpers;

namespace NungSue.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[area]/[controller]")]
    public class RoleController : Controller
    {
        private readonly NungSueContext _context;

        public RoleController(NungSueContext context)
        {
            _context = context;
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var permissionsCount = GetAllPermissions().Count;
            var roles = await _context.Roles
                .Include("UserRoles.User")
                .AsNoTracking()
                .Select(x => new RoleViewModel
                {
                    RoleId = x.RoleId,
                    Name = x.Name,
                    UserCount = x.UserRoles.Count,
                    PermissionCount = $"{x.RolePermissions.Count} / {permissionsCount}"
                })
                .ToListAsync();

            return View(roles);
        }

        [Route("Create")]
        [Route("Edit/{roleId:guid}")]
        public async Task<IActionResult> Create(Guid roleId)
        {
            var allPermissions = GetAllPermissions();
            var viewModel = new RoleCreateViewModel { RolePermissions = allPermissions };
            if (roleId == Guid.Empty) return View(viewModel);

            var role = await _context.Roles
                .AsNoTracking()
                .Include(x => x.RolePermissions)
                .SingleOrDefaultAsync(x => x.RoleId == roleId);
            if (role == null) return NotFound();

            viewModel.RoleId = role.RoleId;
            viewModel.RoleName = role.Name;
            foreach (var permission in viewModel.RolePermissions)
            {
                permission.Selected = role.RolePermissions.Any(x => x.Value == permission.Value);
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route("Create")]
        [Route("Edit/{roleId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid roleId, RoleCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (roleId == Guid.Empty)
            {
                var newRole = new Role
                {
                    Name = model.RoleName,
                    RolePermissions = model.RolePermissions
                   .Where(x => x.Selected)
                   .Select(x => new RolePermission { Value = x.Value })
                   .ToHashSet()
                };
                await _context.Roles.AddAsync(newRole);
            }
            else
            {
                if (roleId != model.RoleId) return BadRequest();

                var role = await _context.Roles
                    .Include(x => x.RolePermissions)
                    .SingleOrDefaultAsync(x => x.RoleId == model.RoleId);

                if (role == null) return NotFound();

                role.Name = model.RoleName;
                role.RolePermissions = model.RolePermissions
                    .Where(x => x.Selected)
                    .Select(x => new RolePermission { Value = x.Value })
                    .ToHashSet();

                _context.Roles.Update(role);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        #region Helper

        private static List<RolePermissionViewModel> GetAllPermissions()
        {
            var allPermissions = new List<RolePermissionViewModel>();
            allPermissions.GetPermissions(typeof(Permissions.Role));
            allPermissions.GetPermissions(typeof(Permissions.User));
            return allPermissions;
        }

        #endregion
    }
}