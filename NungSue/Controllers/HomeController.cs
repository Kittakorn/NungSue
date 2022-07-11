using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NungSue.Constants;
using NungSue.Entities;
using NungSue.Models;
using NungSue.ViewModels;
using System.Diagnostics;
using System.Linq.Dynamic.Core;

namespace NungSue.Controllers
{
    [Authorize(AuthenticationSchemes = AuthSchemes.CustomerAuth)]
    public class HomeController : Controller
    {
        private readonly BookStoreContext _context;
        private readonly IConfiguration _config;


        public HomeController(BookStoreContext context, IConfiguration config)
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

            var books = await _context.Books
                .Include(include)
                .Include(x => x.PriceOffer)
                .Where(x => type == "category" ? x.Category.Name == name : x.BookTags.Any(x => x.Tag.Name == name))
                .Select(x => new BookItem
                {
                    Description = x.Description,
                    Title = x.Title,
                    Price = x.Price.ToString("N0"),
                    PromotionPrice = x.PriceOffer == null ? null : x.PriceOffer.NewPrice.ToString("N0"),
                    PromotionText = x.PriceOffer == null ? null : x.PriceOffer.PromotionText,
                    BookImageUrl = _config.GetValue<string>("ImageUrl") + x.BookImage
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

        [Route("favorite/{bookId:guid}")]
        public async Task<IActionResult> UpdateFavoriteBook(Guid BookId)
        {
            return View();
        }

        [Route("order")]
        public async Task<IActionResult> Order()
        {
            return View();
        }

        [Route("cart/{bookId:guid}")]
        public async Task<IActionResult> AddBookToCart(Guid bookId)
        {
            return View();
        }
    }
}