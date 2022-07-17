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
using System.Linq.Dynamic.Core;

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
    public async Task<IActionResult> Address()
    {
        var customer = await GetCustomer();
        var address = await _context.CustomerAddresses
            .Where(x => x.CustomerId == customer.CustomerId)
            .Select(x => new AddressViewModel
            {
                AddressId = x.AddressId,
                FullName = $"{x.FirstName} {x.LastName}",
                PhoneNumber = x.PhoneNumber.Insert(2, "-").Insert(7, "-"),
                IsDefault = x.IsDefault,
                FullAddress = $"{x.Address} ตำบล{x.District} อำเภอ{x.SubDistrict} จังหวัด{x.Province} {x.ZipCode}"
            })
            .ToListAsync();

        return View(address);
    }

    [Route("address/create")]
    [Route("address/update/{line:int}")]
    public async Task<IActionResult> AddressCreateOrUpdate(int? line)
    {


        var customer = await GetCustomer();
        var customerAddress = await _context.CustomerAddresses.Where(x => x.CustomerId == customer.CustomerId).ToListAsync();

        if (!line.HasValue)
        {
            return View(new AddressCreateViewModel() { FirstAddress = customerAddress.Count == 0 });
        }

        if (line.Value > customerAddress.Count)
            return NotFound();

        var address = customerAddress[line.Value - 1];

        var viewModel = new AddressCreateViewModel
        {
            AddressId = address.AddressId,
            FirstName = address.FirstName,
            LastName = address.LastName,
            PhoneNumber = address.PhoneNumber,
            Address = address.Address,
            District = address.District,
            SubDistrict = address.SubDistrict,
            Province = address.Province,
            ZipCode = address.ZipCode,
            IsDefault = address.IsDefault
        };

        return View(viewModel);
    }

    [HttpPost]
    [Route("address/create")]
    [Route("address/update/{line:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddressCreateOrUpdate(AddressCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var address = new CustomerAddress();
        var customer = await GetCustomer();
        var customerAddress = await _context.CustomerAddresses.Where(x => x.CustomerId == customer.CustomerId).ToListAsync();

        if (model.IsDefault)
        {
            customerAddress.ForEach(x => x.IsDefault = false);
        }
        else if (customerAddress.Count == 0)
        {
            model.IsDefault = true;
        }

        if (model.AddressId != Guid.Empty)
        {
            address = await _context.CustomerAddresses.FindAsync(model.AddressId);
        }

        address.FirstName = model.FirstName;
        address.LastName = model.LastName;
        address.PhoneNumber = model.PhoneNumber.Replace("-", "");
        address.Address = model.Address;
        address.District = model.District;
        address.SubDistrict = model.SubDistrict;
        address.Province = model.Province;
        address.ZipCode = model.ZipCode;
        address.CustomerId = customer.CustomerId;
        address.IsDefault = model.IsDefault;


        if (model.AddressId != Guid.Empty)
            _context.CustomerAddresses.Update(address);
        else
            _context.CustomerAddresses.Add(address);

        await _context.SaveChangesAsync();
        _notify.Success("เพิ่มที่อยู่สำเร็จ");

        return RedirectToAction("Address");
    }

    [Route("address/delete/{addressId:guid}")]
    public async Task<IActionResult> DeleteAddress(Guid addressId)
    {
        var address = await _context.CustomerAddresses.FindAsync(addressId);
        if (address == null)
            return NotFound();

        _context.CustomerAddresses.Remove(address);
        await _context.SaveChangesAsync();

        _notify.Success("ลบอยู่ที่แล้ว");
        return RedirectToAction("Address");
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

        var query = _context.Favorites
            .Include(x => x.Book)
            .ThenInclude(x => x.PriceOffer)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreateDate);

        var books = await query.Select(x => new BookItem
        {
            BookId = x.BookId,
            Description = x.Book.Description,
            Title = x.Book.Title,
            Price = x.Book.Price.ToString("N0"),
            PromotionPrice = x.Book.PriceOffer == null ? null : x.Book.PriceOffer.NewPrice.ToString("N0"),
            PromotionText = x.Book.PriceOffer == null ? null : x.Book.PriceOffer.PromotionText,
            BookImageUrl = _config.GetValue<string>("ImageUrl") + x.Book.BookImage
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
