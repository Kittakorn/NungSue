using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NungSue.Areas.Admin.ViewModels.User;
using NungSue.Constants;
using NungSue.Entities;
using NungSue.Extensions;
using NungSue.Interfaces;
using System.Linq.Dynamic.Core;

namespace NungSue.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[area]/[controller]")]
    [Authorize(AuthenticationSchemes = AuthSchemes.UserAuth)]
    public class UserController : Controller
    {
        private readonly BookStoreContext _context;
        private readonly IConfiguration _config;
        private readonly IBlobService _blobService;

        public UserController(BookStoreContext context, IConfiguration config, IBlobService blobService)
        {
            _context = context;
            _config = config;
            _blobService = blobService;
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("GetUsers")]
        public async Task<IActionResult> GetUsers()
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
                var users = _context.Users.AsNoTracking().Include("UserRoles.Role");

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    users = users.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                else
                {
                    users = users.OrderByDescending(x => x.IsActive)
                        .ThenByDescending(x => x.FirstName);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    users = users.Where(x => x.Username.Contains(searchValue) ||
                                         x.FirstName.Contains(searchValue) ||
                                         x.LastName.Contains(searchValue) ||
                                         x.PhoneNumber.Contains(searchValue) ||
                                         (x.IsActive.Value ? "ใช้งาน" : "ไม่ได้ใช้งาน").Contains(searchValue) ||
                                         x.UserRoles.Any(x => x.Role.Name.Contains(searchValue)) ||
                                         _context.DateToString(x.CreateDate, "dd/MM/yyyy HH:mm").First().DateString.Contains(searchValue) ||
                                         _context.DateToString(x.LastLogin, "dd/MM/yyyy HH:mm").First().DateString.Contains(searchValue));
                }

                recordsTotal = await users.CountAsync();

                var data = await users.Skip(skip).Take(pageSize).Select(x => new UserViewModel
                {
                    UserId = x.UserId,
                    Username = x.Username,
                    FullName = $"{x.FirstName} {x.LastName}",
                    PhoneNumber = x.PhoneNumber.Insert(2, "-").Insert(7, "-"),
                    CreateDate = x.CreateDate.ToThaiString("dd/MM/yyyy HH:mm"),
                    LastLogin = x.LastLogin.HasValue ? x.LastLogin.Value.ToThaiString("dd/MM/yyyy HH:mm") : null,
                    IsActive = x.IsActive!.Value,
                    Roles = string.Join(", ", x.UserRoles.Select(x => x.Role.Name))
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
        [Route("Edit/{userId:guid}")]
        public async Task<IActionResult> Create(Guid userId)
        {
            ViewBag.Roles = new SelectList(_context.Roles.AsNoTracking(), "RoleId", "Name");

            if (userId == Guid.Empty)
                return View(new UserCreateViewModel());

            var user = await _context.Users
                       .Include("UserRoles.Role")
                       .Select(x => new UserCreateViewModel
                       {
                           UserId = x.UserId,
                           Username = x.Username,
                           FirstName = x.FirstName,
                           LastName = x.LastName,
                           PhoneNumber = x.PhoneNumber,
                           IsActive = x.IsActive.Value,
                           RoleIds = x.UserRoles.Select(x => x.RoleId).ToList(),
                           ProfileImageUrl = _config.GetValue<string>("ImageUrl") + x.ProfileImage
                       })
                       .SingleOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [Route("Create")]
        [Route("Edit/{userId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid userId, UserCreateViewModel model)
        {
            if (userId != Guid.Empty && userId != model.UserId)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(_context.Roles.AsNoTracking(), "RoleId", "Name");
                return View(model);
            }

            var user = new User();

            if (userId == Guid.Empty)
            {
                user.Username = model.Username;
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.PhoneNumber.Replace("-", ""));
            }
            else
            {
                user = await _context.Users
                     .Include("UserRoles.Role")
                     .SingleOrDefaultAsync(x => x.UserId == model.UserId);

                if (user == null)
                    return BadRequest();

                user.UpdateDate = DateTime.Now;
            }

            if (model.ProfileImage != null)
            {
                var fileName = @"user/" + user.Username + Path.GetExtension(model.ProfileImage.FileName);
                user.ProfileImage = await _blobService.UploadFileBlobAsync(fileName, model.ProfileImage, "image");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber.Replace("-", "");
            user.IsActive = model.IsActive;
            user.UserRoles = model.RoleIds.Select(x => new UserRole { RoleId = x }).ToHashSet();


            if (userId == Guid.Empty)
                await _context.Users.AddAsync(user);
            else
                _context.Users.Update(user);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}