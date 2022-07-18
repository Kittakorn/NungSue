using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NungSue.Areas.Admin.ViewModels.Book;
using NungSue.Constants;
using NungSue.Entities;
using NungSue.Extensions;
using NungSue.Interfaces;
using System.Globalization;
using System.Linq.Dynamic.Core;

namespace NungSue.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[area]/[controller]")]
    [Authorize(AuthenticationSchemes = AuthSchemes.UserAuth)]
    public class BookController : Controller
    {
        private readonly NungSueContext _context;
        private readonly IBlobService _blobService;
        private readonly IConfiguration _configuration;

        public BookController(NungSueContext context, IBlobService blobService, IConfiguration configuration)
        {
            _context = context;
            _blobService = blobService;
            _configuration = configuration;
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("GetBooks")]
        public async Task<IActionResult> GetBooks()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                var books = _context.Books.AsNoTracking()
                    .Include(x => x.Category)
                    .Include(x => x.Publisher)
                    .Include(x => x.PriceOffer)
                    .Include("BookTags.Tag")
                    .Include("BookAuthors.Author")
                    .AsQueryable();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    books = (sortColumn, sortColumnDirection) switch
                    {
                        ("Price", "desc") => books.OrderByDescending(x => x.PriceOffer == null ? x.Price : x.PriceOffer.NewPrice),
                        ("Price", "asc") => books.OrderBy(x => x.PriceOffer == null ? x.Price : x.PriceOffer.NewPrice),
                        _ => books.OrderBy(sortColumn + " " + sortColumnDirection)
                    };
                }
                else
                {
                    books = books.OrderByDescending(x => x.IsPublish)
                        .ThenByDescending(x => x.PublishedOn)
                        .ThenByDescending(x => x.Title)
                        .ThenByDescending(x => x.Category.Name);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    books = books.Where(x => x.Barcode.Contains(searchValue) ||
                                         x.Category.Name.Contains(searchValue) ||
                                         x.Title.Contains(searchValue) ||
                                         x.Price.ToString().Contains(searchValue) ||
                                         x.PriceOffer.PromotionText.Contains(searchValue) ||
                                         x.PriceOffer.NewPrice.ToString().Contains(searchValue) ||
                                         x.BookTags.Any(x => x.Tag.Name.Contains(searchValue)) ||
                                         x.BookAuthors.Any(x => x.Author.Name.Contains(searchValue)) ||
                                         (x.IsPublish ? "เปิดขาย" : "ปิดการขาย").Contains(searchValue) ||
                                         _context.DateToString(x.PublishedOn, "dd/MM/yyyy HH:mm").First().DateString.Contains(searchValue));
                }

                recordsTotal = await books.CountAsync();

                var data = await books.Skip(skip).Take(pageSize).Select(x => new BookViewModel
                {
                    BookId = x.BookId,
                    Barcode = x.Barcode,
                    BookCategory = x.Category.Name,
                    Title = x.Title,
                    Price = x.Price.ToString("N0"),
                    PromotionPrice = x.PriceOffer == null ? null : x.PriceOffer.NewPrice.ToString("N0"),
                    PublishedOn = x.PublishedOn.ToThaiString("dd/MM/yyyy HH:mm"),
                    Tags = string.Join(", ", x.BookTags.Select(x => x.Tag.Name)),
                    Authors = string.Join(", ", x.BookAuthors.Select(x => x.Author.Name)),
                    IsPublish = x.IsPublish,
                }).ToListAsync();


                var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
                return Ok(jsonData);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Route("Create")]
        [Route("Edit/{bookId:guid}")]
        public async Task<IActionResult> Create(Guid bookId)
        {
            ViewBag.Tags = new SelectList(_context.Tags.AsNoTracking(), "TagId", "Name");
            ViewBag.Categories = new SelectList(_context.Categories.AsNoTracking(), "CategoryId", "Name");
            ViewBag.Publishers = new SelectList(_context.Publishers.AsNoTracking(), "PublisherId", "Name");
            ViewBag.Authors = new SelectList(_context.Authors.AsNoTracking(), "AuthorId", "Name");

            if (bookId == Guid.Empty)
                return View(new BookCreateViewModel());

            var book = await _context.Books
                .Include(x => x.BookTags)
                .Include(x => x.BookAuthors)
                .Select(x => new BookCreateViewModel
                {
                    BookId = x.BookId,
                    Barcode = x.Barcode,
                    Title = x.Title,
                    Description = x.Description,
                    Content = x.Content,
                    ListOfContents = x.ListOfContents,
                    Price = x.Price.ToString(),
                    Size = x.Size,
                    Weight = x.Weight.ToString(),
                    NumberOfPage = x.NumberOfPage.ToString(),
                    MonthOfPublication = x.MonthOfPublication,
                    PublishedOn = x.PublishedOn,
                    TagIds = x.BookTags.Select(t => t.TagId).ToList(),
                    AuthorIds = x.BookAuthors.Select(a => a.AuthorId).ToList(),
                    CategoryId = x.CategoryId,
                    PublisherId = x.PublisherId,
                    IsPublish = x.IsPublish
                })
                .SingleOrDefaultAsync(x => x.BookId == bookId);

            if (book == null)
                return NotFound();

            return View(book);
        }

        [HttpPost]
        [Route("Create")]
        [Route("Edit/{bookId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid bookId, BookCreateViewModel model)
        {
            if (bookId != Guid.Empty && bookId != model.BookId)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Tags = new SelectList(_context.Tags.AsNoTracking(), "TagId", "Name");
                ViewBag.Categories = new SelectList(_context.Categories.AsNoTracking(), "CategoryId", "Name");
                ViewBag.Publishers = new SelectList(_context.Publishers.AsNoTracking(), "PublisherId", "Name");
                ViewBag.Authors = new SelectList(_context.Authors.AsNoTracking(), "AuthorId", "Name");

                return View(model);
            }

            var book = new Book();

            if (bookId != Guid.Empty)
            {
                book = await _context.Books
                    .Include(x => x.BookAuthors)
                    .Include(x => x.BookTags)
                    .SingleOrDefaultAsync(x => x.BookId == bookId);

                if (book == null)
                    return BadRequest();

                book.UpdateBy = Guid.Parse(User.FindFirst("UserId").Value);
                book.UpdateDate = DateTime.Now;
                book.BookAuthors.Clear();
                book.BookTags.Clear();

                if (model.BookImage != null)
                {
                    await _blobService.DeleteBlobAsync(book.Barcode, "book");
                    book.BookImage = await _blobService.UploadFileBlobAsync(book.Barcode, model.BookImage, "book");
                }
            }
            else
            {
                book.Barcode = model.Barcode;
                book.CreateBy = Guid.Parse(User.FindFirst("UserId").Value);
                book.BookImage = await _blobService.UploadFileBlobAsync(model.Barcode, model.BookImage, "book");
            }

            book.Title = model.Title;
            book.Description = model.Description;
            book.Content = model.Content;
            book.ListOfContents = model.ListOfContents;
            book.Size = model.Size;
            book.Weight = int.Parse(model.Weight, NumberStyles.AllowThousands);
            book.NumberOfPage = int.Parse(model.NumberOfPage, NumberStyles.AllowThousands);
            book.MonthOfPublication = model.MonthOfPublication;
            book.Price = int.Parse(model.Price, NumberStyles.AllowThousands);
            book.PublishedOn = model.PublishedOn;
            book.CategoryId = model.CategoryId;
            book.PublisherId = model.PublisherId;
            book.IsPublish = model.IsPublish;

            model.TagIds.ForEach(id => book.BookTags.Add(new BookTag { TagId = id }));
            model.AuthorIds.ForEach(id => book.BookAuthors.Add(new BookAuthor { AuthorId = id }));

            if (bookId != Guid.Empty)
                _context.Books.Update(book);
            else
                await _context.Books.AddAsync(book);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Route("Details/{bookId:guid}")]
        public async Task<IActionResult> Details(Guid bookId)
        {
            var book = await _context.Books
                .AsNoTracking()
                .Include("Reviews.User")
                .Include("BookTags.Tag")
                .Include("BookAuthors.Author")
                .Include(x => x.PriceOffer)
                .Include(x => x.Category)
                .Include(x => x.Publisher)
                .Select(x => new BookDetailsViewModel
                {
                    BookId = x.BookId,
                    ImageUrl = _configuration.GetValue<string>("ImageUrl") + "book/" + x.Barcode,
                    Barcode = x.Barcode,
                    Title = x.Title,
                    Description = x.Description,
                    Content = x.Content,
                    ListOfContent = x.ListOfContents,
                    Size = x.Size,
                    Weight = x.Weight.ToString("N0"),
                    NumberOfPage = x.NumberOfPage.ToString("N0"),
                    Price = x.Price.ToString("N0"),
                    MonthOfPublication = x.MonthOfPublication.ToThaiString("MM/yyyy"),
                    PublishedOn = x.PublishedOn.ToThaiString("dd/MM/yyyy HH:mm"),
                    CreateBy = x.CreateByNavigation.FirstName + " " + x.CreateByNavigation.LastName,
                    CreateDate = x.CreateDate.ToThaiString("dd/MM/yyyy HH:mm"),
                    UpdateBy = x.UpdateByNavigation == null ? null : x.UpdateByNavigation.FirstName + " " + x.UpdateByNavigation.LastName,
                    UpdateDate = x.UpdateDate == null ? null : x.UpdateDate.Value.ToThaiString("dd/MM/yyyy HH:mm"),
                    Category = x.Category.Name,
                    Publisher = x.Publisher.Name,
                    IsPublish = x.IsPublish ? "กำลังวางจำหน่าย" : "ไม่ได้จำหน่าย",
                    Tags = string.Join(", ", x.BookTags.Select(x => x.Tag.Name)),
                    Authors = string.Join(", ", x.BookAuthors.Select(x => x.Author.Name)),
                    NewPrice = x.PriceOffer == null ? null : x.PriceOffer.NewPrice.ToString("N0"),
                    PromotionText = x.PriceOffer == null ? null : x.PriceOffer.PromotionText
                })
                .SingleOrDefaultAsync(x => x.BookId == bookId);

            if (book == null)
                return NotFound();

            return View(book);
        }
    }
}