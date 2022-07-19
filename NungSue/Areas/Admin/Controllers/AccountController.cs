using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NungSue.Areas.Admin.ViewModels.Account;
using NungSue.Constants;
using NungSue.Entities;
using NungSue.Models;
using System.Security.Claims;

namespace NungSue.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[area]/[controller]")]
    [Authorize(AuthenticationSchemes = AuthSchemes.UserAuth)]
    public class AccountController : Controller
    {
        private readonly NungSueContext _context;

        public AccountController(NungSueContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [Route("/Admin/SignIn")]
        public IActionResult SignIn(string returnUrl)
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("/Admin/SignIn")]
        public async Task<IActionResult> SignIn(SignInViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.SingleOrDefaultAsync(x => x.Username == model.Username);
            if (user == null)
            {
                ModelState.AddModelError("", "ไม่พบชื่อผู้ใช้งานนี้ในระบบ");
                return View(model);
            }

            var passwordIsMatch = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
            if (!passwordIsMatch)
            {
                ModelState.AddModelError("", "รหัสผ่านไม่ถูกต้อง กรุณาลองใหม่อีกครั้ง");
                return View(model);
            }

            user.LastLogin = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var permissions = new List<string>();
            var userRoles = await _context.UserRoles.AsNoTracking().Where(x => x.UserId == user.UserId).ToListAsync();
            foreach (var role in userRoles)
            {
                var rolePermissions = await _context.RolePermissions
                    .Where(x => x.RoleId == role.RoleId)
                    .Select(x => x.Value)
                    .ToListAsync();
                permissions.AddRange(rolePermissions);
            }

            var claims = new List<Claim>
            {
                new Claim("UserId", user.UserId.ToString()),
                new Claim("FullName", $"{user.FirstName} {user.LastName}")
            };
            claims.AddRange(permissions.Distinct().Select(x => new Claim(ClaimTypes.Role, x)));

            var authProperties = new AuthenticationProperties { IsPersistent = model.RememberMe };
            var claimsIdentity = new ClaimsIdentity(claims, AuthSchemes.UserAuth);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(AuthSchemes.UserAuth, claimsPrincipal, authProperties);

            return RedirectToLocal(returnUrl);
        }

        [AllowAnonymous]
        public async Task<IActionResult> SignOutAsync()
        {
            await HttpContext.SignOutAsync(AuthSchemes.UserAuth);
            return LocalRedirect("/");
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        #region Helper
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
        #endregion
    }
}
