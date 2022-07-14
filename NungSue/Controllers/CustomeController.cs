using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NungSue.Constants;
using NungSue.Entities;
using System.Net;
using System.Security.Claims;

namespace NungSue.Controllers;

[Route("customer")]
[Authorize(AuthenticationSchemes = AuthSchemes.CustomerAuth)]
public class CustomerController : Controller
{
    private readonly NungSueContext _context;

    public CustomerController(NungSueContext context)
    {
        _context = context;
    }
    
    [Route("")]
    public IActionResult Profile()
    {
        return View();
    }

    [Route("favorite/{bookId:guid}")]
    public async Task<JsonResult> UpdateFavoriteBook(Guid bookId)
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

}
