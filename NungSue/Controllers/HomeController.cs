using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NungSue.Constants;
using NungSue.Entities;
using NungSue.Models;
using NungSue.ViewModels;
using System.Diagnostics;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace NungSue.Controllers;

public class HomeController : Controller
{
    private readonly NungSueContext _context;
    private readonly IConfiguration _config;

    public HomeController(NungSueContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [Route("")]
    public IActionResult Index()
    {
        return View();
    }

    [Route("category/{name}")]
    [Route("tag/{name}")]
    public async Task<IActionResult> CategoryTag(string name)
    {
        var type = Request.Path.Value.Split("/")[1].ToLower();
        name = name.Replace("-", " ");

        Category findCategory = null;
        Tag findTag = null;

        if (type == "category")
            findCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == name);
        else
            findTag = await _context.Tags.FirstOrDefaultAsync(c => c.Name == name);

        if (findCategory == null && findTag == null)
            return NotFound();

        var include = type == "category" ? "Category" : "BookTags.Tag";

        var query = _context.Books
            .Include(include)
            .Include(x => x.PriceOffer)
            .Include(x => x.Favorites)
            .Where(x => type == "category" ? x.Category.Name == name : x.BookTags.Any(x => x.Tag.Name == name));

        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var customerId = id == null ? Guid.Empty : Guid.Parse(id);

        var books = await query.Select(x => new BookItem
        {
            BookId = x.BookId,
            Description = x.Description,
            Title = x.Title,
            Price = x.Price.ToString("N0"),
            PromotionPrice = x.PriceOffer == null ? null : x.PriceOffer.NewPrice.ToString("N0"),
            PromotionText = x.PriceOffer == null ? null : x.PriceOffer.PromotionText,
            BookImageUrl = _config.GetValue<string>("ImageUrl") + x.BookImage,
            IsFavorite = x.Favorites.Any(f => f.CustomerId == customerId)
        }).ToListAsync();

        var data = new CategoryTagViewModel
        {
            Name = name,
            BookItems = books,
            TotalRecord = books.Count.ToString("N0"),
            Type = type
        };

        return View(data);
    }

    [Route("like/{bookId:guid}")]
    public async Task<JsonResult> LikeBook(Guid bookId)
    {
        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var favorite = await _context.Favorites.FirstOrDefaultAsync(x => x.CustomerId == customerId && x.BookId == bookId);

        if (favorite == null)
        {
            var newFavorite = new Favorite
            {
                BookId = bookId,
                CustomerId = customerId
            };
            await _context.Favorites.AddAsync(newFavorite);
        }
        else
        {
            _context.Favorites.Remove(favorite);
        }

        await _context.SaveChangesAsync();
        return Json(favorite == null);
    }

    [Route("cart/add/{bookId:guid}")]
    public async Task<JsonResult> AddBookToCart(Guid bookId)
    {
        var customer = await GetCustomer();
        var shoppingCart = await _context.ShoppingCarts.FirstOrDefaultAsync(x => x.BookId == bookId && x.CustomerId == customer.CustomerId);
        if (shoppingCart == null)
        {
            shoppingCart = new ShoppingCart();
            shoppingCart.BookId = bookId;
            shoppingCart.CustomerId = customer.CustomerId;
            shoppingCart.CreateDate = DateTime.Now;
            shoppingCart.Quantity = 1;
            await _context.AddAsync(shoppingCart);
        }
        else
        {
            shoppingCart.Quantity++;
            _context.Update(shoppingCart);
        }

        await _context.SaveChangesAsync();
        var count = await _context.ShoppingCarts.CountAsync(x => x.CustomerId == customer.CustomerId);
        return Json(count);
    }

    private async Task<Customer> GetCustomer()
    {
        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var customer = await _context.Customers.FindAsync(customerId);
        return customer;
    }

}