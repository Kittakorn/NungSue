using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NungSue.Constants;
using NungSue.Entities;
using NungSue.Models;
using NungSue.ViewModels;
using System.Net;
using System.Security.Claims;

namespace NungSue.Controllers;

[Route("user")]
[Authorize(AuthenticationSchemes = AuthSchemes.CustomerAuth)]
public class UserController : Controller
{
    private readonly NungSueContext _context;
    private readonly IConfiguration _config;
    private readonly INotyfService _notify;


    public UserController(NungSueContext context, IConfiguration config, INotyfService notify)
    {
        _context = context;
        _config = config;
        _notify = notify;
    }

    [Route("profile")]
    public IActionResult Profile()
    {
        return View();
    }

    [Route("address")]
    public IActionResult Address()
    {
        return View();
    }

    [Route("purchase")]
    public IActionResult Purchase()
    {
        return View();
    }

    [Route("password")]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Route("password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) 
            return View(model);
        
        var customer = await GetCustomer();

        var passwordIsMatch = BCrypt.Net.BCrypt.Verify(model.OldPassword, customer.Password);
        if (!passwordIsMatch)
        {
            ModelState.AddModelError("OldPassword", "รหัสผ่านปัจจุบันไม่ถูกต้อง");
            return View(model);
        }

        customer.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();

        _notify.Success("เปลี่ยนรหัสผ่านสำเร็จ");
        return RedirectToAction("ChangePassword");

    }

    [Route("set-password")]
    public async Task<IActionResult> SetPassword()
    {
        var customer = await GetCustomer();

        if (customer.Password != null) 
            return RedirectToAction("ChangePassword");
        
        return View();
    }

    [HttpPost]
    [Route("set-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var customer = await GetCustomer();
        customer.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();

        _notify.Success("เพิ่มรหัสผ่านสำเร็จ");
        return RedirectToAction("ChangePassword");
    }

    [Route("favorite")]
    public async Task<IActionResult> Favorite()
    {
        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var query = _context.Books
            .Include(x => x.PriceOffer)
            .Include(x => x.Favorites)
            .Where(x => x.Favorites.Any(f => f.CustomerId == customerId));

        var books = await query.Select(x => new BookItem
        {
            BookId = x.BookId,
            Description = x.Description,
            Title = x.Title,
            Price = x.Price.ToString("N0"),
            PromotionPrice = x.PriceOffer == null ? null : x.PriceOffer.NewPrice.ToString("N0"),
            PromotionText = x.PriceOffer == null ? null : x.PriceOffer.PromotionText,
            BookImageUrl = _config.GetValue<string>("ImageUrl") + x.BookImage
        }).ToListAsync();

        return View(books);
    }

    [Route("history")]
    public IActionResult History()
    {
        return View();
    }

    private async Task<Customer> GetCustomer()
    {
        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var customer = await _context.Customers.FindAsync(customerId);
        return customer;
    }


}
