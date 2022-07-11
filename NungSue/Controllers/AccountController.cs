using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NungSue.Constants;
using NungSue.Entities;
using NungSue.ViewModels;
using System.Security.Claims;

namespace NungSue.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly NungSueContext _context;
        public INotyfService _notify;

        public AccountController(NungSueContext context, INotyfService notify)
        {
            _context = context;
            _notify = notify;
        }

        [Route("sign-in")]
        public IActionResult SignIn(string returnUrl)
        {
            if (User.Identity.IsAuthenticated && User.Identity.AuthenticationType == AuthSchemes.CustomerAuth)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [Route("sign-in")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(string returnUrl, SignInViewModel model)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Email == model.Email);
            if (customer == null)
            {
                ModelState.AddModelError("Email", "ไม่พบอีเมลนี้ในระบบ");
                return View(model);
            }

            var passwordIsMatch = BCrypt.Net.BCrypt.Verify(model.Password, customer.Password);
            if (!passwordIsMatch)
            {
                ModelState.AddModelError("Password", "รหัสผ่านไม่ถูกต้อง กรุณาตรวจสอบอีกครั้ง");
                return View(model);
            }

            customer.LastLogin = DateTime.Now;
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            await SignInCustomer(customer.Email, customer.CustomerId, $"{customer.FirstName} {customer.LastName}", model.RememberMe);
            _notify.Success("เข้าสู่ระบบสำเร็จ");

            if (returnUrl != null)
                return RedirectToLocal(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Route("register")]
        public IActionResult Register()
        {

            return View();
        }

        [HttpPost]
        [Route("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var customer = new Customer
            {
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                LastLogin = DateTime.Now
            };

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            await SignInCustomer(customer.Email, customer.CustomerId, "");

            _notify.Success("สมัครสมาชิกสำเร็จ");

            return RedirectToAction("Index", "Home");
        }

        [Route("sign-out")]
        [ActionName("SignOut")]
        public async Task<IActionResult> SignOutAsync()
        {
            await HttpContext.SignOutAsync(AuthSchemes.CustomerAuth);
            _notify.Success("ออกจากระบบสำเร็จ");
            return RedirectToAction("SignIn");
        }


        [AllowAnonymous]
        [HttpPost(nameof(ExternalLogin))]
        public IActionResult ExternalLogin(string provider)
        {
            var properties = new AuthenticationProperties { RedirectUri = "ExternalLoginCallback" };

            return Challenge(properties, provider);
        }

        [AllowAnonymous]
        [HttpGet(nameof(ExternalLoginCallback))]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        {
            //Here we can retrieve the claims
            var result = await HttpContext.AuthenticateAsync(AuthSchemes.ExternalAuth);
            await SignInCustomer("test@gmail.com", new Guid(), "test");
            return RedirectToAction("Index");
        }


        private async Task SignInCustomer(string email, Guid customerId, string name, bool isRemember = false)
        {
            var claims = new List<Claim>
            {
                new Claim("Email", email),
                new Claim("CustomerId",customerId.ToString()),
                new Claim(ClaimTypes.Name,name)
            };

            var authProperties = new AuthenticationProperties { IsPersistent = isRemember };
            var claimsIdentity = new ClaimsIdentity(claims, AuthSchemes.CustomerAuth);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(AuthSchemes.CustomerAuth, claimsPrincipal, authProperties);
        }

        #region Helper
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}
