using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NungSue.Constants;
using NungSue.Entities;
using NungSue.Extensions;
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
            .Where(x => (type == "category" ? x.Category.Name == name : x.BookTags.Any(x => x.Tag.Name == name)) && x.IsPublish);

        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var customerId = id == null ? Guid.Empty : Guid.Parse(id);

        var books = await query.Select(x => new BookItem
        {
            BookId = x.BookId,
            Barcode = x.Barcode,
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

    [Route("product/{barcode}")]
    public async Task<IActionResult> BookDetail(string barcode)
    {
        var customer = await GetCustomer();

        var book = await _context.Books
            .Include(x => x.BookTags)
            .Include(x => x.BookAuthors)
            .Include(x => x.Category)
            .Include(x => x.Publisher)
            .Include(x => x.Favorites.Where(x => x.CustomerId == customer.CustomerId && customer != null))
            .Include(x => x.PriceOffer)
            .Where(x => x.IsPublish)
            .Select(x => new BookDetailViewModel
            {
                BookId = x.BookId,
                Barcode = x.Barcode,
                Title = x.Title,
                Description = x.Description,
                Content = x.Content,
                ListOfContents = x.ListOfContents,
                Size = x.Size,
                Weight = x.Weight.ToString("N0"),
                NumberOfPage = x.NumberOfPage.ToString("N0"),
                MonthOfPublication = x.MonthOfPublication.ToThaiString("MM/yyyy"),
                Price = x.Price.ToString("N0"),
                NewPrice = x.PriceOffer == null ? null : x.PriceOffer.NewPrice.ToString("N0"),
                PromotionText = x.PriceOffer == null ? null : x.PriceOffer.PromotionText,
                Category = x.Category.Name,
                Publisher = x.Publisher.Name,
                Tags = x.BookTags.Select(x => x.Tag.Name).ToList(),
                Authors = x.BookAuthors.Select(x => x.Author.Name).ToList(),
                BookImage = _config.GetValue<string>("ImageUrl") + x.BookImage,
                IsFavorite = x.Favorites.Count != 0,
                IsPublish = x.PublishedOn >= DateTime.Now,
                PublishedOn = x.PublishedOn.ToThaiString("dd/MM/yyyy HH:mm")
            })
            .FirstOrDefaultAsync(x => x.Barcode == barcode);

        if (book == null)
            return NotFound();

        await AddBookToHistory(book.BookId);
        return View();
    }

    private async Task<Customer> GetCustomer()
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (customerId != null)
        {
            var customer = await _context.Customers.FindAsync(Guid.Parse(customerId));
            return customer;
        }

        return null;
    }

    private async Task AddBookToHistory(Guid bookId)
    {
        var customer = await GetCustomer();
        if (customer == null)
            return;

        var history = await _context.Histories.FirstOrDefaultAsync(x => x.CustomerId == customer.CustomerId && x.BookId == bookId);

        if (history != null)
        {
            history.CreateDate = DateTime.Now;
            _context.Histories.Update(history);
        }
        else
        {
            history = new History
            {
                BookId = bookId,
                CustomerId = customer.CustomerId,
                CreateDate = DateTime.Now
            };

            var count = await _context.Histories.CountAsync(x => x.CustomerId == customer.CustomerId);
            if (count == 20)
            {
                var oldVisit = await _context.Histories
                    .OrderBy(x => x.CreateDate)
                    .FirstOrDefaultAsync(x => x.CustomerId == customer.CustomerId);

                _context.Histories.Remove(oldVisit);
            }

            await _context.Histories.AddAsync(history);
        }

        await _context.SaveChangesAsync();
    }
}