using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NungSue.ActionFilters;
using NungSue.Constants;
using NungSue.Entities;
using NungSue.Helpers;
using NungSue.Interfaces;
using NungSue.ViewModels;
using System.Security.Claims;

namespace NungSue.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly NungSueContext _context;
        private readonly INotyfService _notify;
        private readonly IBlobService _blobService;

        public AccountController(NungSueContext context, INotyfService notify, IBlobService blobService)
        {
            _context = context;
            _notify = notify;
            _blobService = blobService;
        }

        [IsAuthorize]
        [Route("sign-in")]
        public IActionResult SignIn(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [IsAuthorize]
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

            if (customer.Password != null)
            {
                var passwordIsMatch = BCrypt.Net.BCrypt.Verify(model.Password, customer.Password);
                if (!passwordIsMatch)
                {
                    ModelState.AddModelError("Password", "รหัสผ่านไม่ถูกต้อง กรุณาตรวจสอบอีกครั้ง");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("Password", "รหัสผ่านไม่ถูกต้อง กรุณาตรวจสอบอีกครั้ง");
                return View(model);
            }

            customer.LastLogin = DateTime.Now;
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            await SignInCustomerAsync(customer, model.RememberMe);
            _notify.Success("เข้าสู่ระบบสำเร็จ");

            return RedirectToLocal(returnUrl);
        }

        [IsAuthorize]
        [Route("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [IsAuthorize]
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
            await SignInCustomerAsync(customer);

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

        [IsAuthorize]
        [HttpPost(nameof(ExternalLogin))]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        [IsAuthorize]
        [HttpGet(nameof(ExternalLoginCallback))]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        {
            var info = await HttpContext.AuthenticateAsync(AuthSchemes.ExternalAuth);

            var providerName = info.Principal.Identity.AuthenticationType;
            var providerKey = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

            var loginProvider = await _context.CustomerLogins
                .FirstOrDefaultAsync(x => x.LoginProvider == providerName && x.ProviderKey == providerKey);

            if (loginProvider == null)
                return RedirectToAction("ExternalLoginConfirm", new { returnUrl });

            var customer = await _context.Customers.FindAsync(loginProvider.CustomerId);
            customer.LastLogin = DateTime.Now;
            _context.Customers.Update(customer);
            _context.SaveChanges();

            await SignInCustomerAsync(customer);
            _notify.Success("เข้าสู่ระบบสำเร็จ");

            return RedirectToLocal(returnUrl);
        }

        [IsAuthorize]
        [Route("account/confirm")]
        public async Task<IActionResult> ExternalLoginConfirm()
        {
            var info = await HttpContext.AuthenticateAsync(AuthSchemes.ExternalAuth);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            var viewModel = new RegisterConfirmViewModel() { Email = email };

            return View(viewModel);
        }

        [HttpPost]
        [IsAuthorize]
        [Route("account/confirm")]
        public async Task<IActionResult> ExternalLoginConfirm(RegisterConfirmViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var info = await HttpContext.AuthenticateAsync(AuthSchemes.ExternalAuth);

            var providerName = info.Principal.Identity.AuthenticationType;
            var providerKey = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

            string profileImage = null;
            if (providerName == "Facebook")
                profileImage = $"https://graph.facebook.com/{providerKey}/picture?type=large";
            else if (providerName == "Google")
                profileImage = info.Principal.FindFirstValue(ClaimTypes.Actor);

            var customer = new Customer
            {
                Email = model.Email,
                FirstName = firstName,
                LastName = lastName,
                LastLogin = DateTime.Now,
                CustomerLogins = new List<CustomerLogin>
                {
                    new CustomerLogin
                    {
                        LoginProvider = providerName,
                        ProviderKey = providerKey,
                    }
                }
            };

            if (profileImage != null)
            {
                var fileName = @"customer/" + DateTime.Now.ToString("yyyyMMddHHmmss");
                customer.ProfileImage = await _blobService.UploadFileBlobAsync(fileName, profileImage, "image");
            }

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            await SignInCustomerAsync(customer);
            _notify.Success("เข้าสู่ระบบสำเร็จ");

            return RedirectToLocal(returnUrl);
        }

        private async Task SignInCustomerAsync(Customer info, bool isRemember = false)
        {
            var fullName = $"{info.FirstName} {info.LastName}".Trim();
            if (string.IsNullOrEmpty(fullName))
                fullName = info.Email;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, info.Email),
                new Claim(ClaimTypes.NameIdentifier,info.CustomerId.ToString()),
                new Claim(ClaimTypes.Name,fullName)
            };

            var authProperties = new AuthenticationProperties { IsPersistent = isRemember };
            var claimsIdentity = new ClaimsIdentity(claims, AuthSchemes.CustomerAuth);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignOutAsync(AuthSchemes.ExternalAuth);
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
