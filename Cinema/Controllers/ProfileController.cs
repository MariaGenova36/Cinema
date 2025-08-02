using Cinema.Models;
using CinemaProjections.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    private async Task<List<Ticket>> GetUserTicketsAsync(string userId)
    {
        return await _context.Tickets
            .Include(t => t.Projection)
                .ThenInclude(p => p.Movie)
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        var model = new ProfileViewModel
        {
            Email = user.Email,
            FullName = user.FullName
        };

        ViewBag.Tickets = await GetUserTicketsAsync(user.Id);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.Tickets = await GetUserTicketsAsync(user.Id);
            return View(model);
        }

        var userToUpdate = await _userManager.GetUserAsync(User);
        userToUpdate.FullName = model.FullName;

        var result = await _userManager.UpdateAsync(userToUpdate);

        if (result.Succeeded)
        {
            ViewData["Message"] = "Profile updated successfully.";
            ViewBag.Tickets = await GetUserTicketsAsync(userToUpdate.Id);
            return View(model);
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        ViewBag.Tickets = await GetUserTicketsAsync(userToUpdate.Id);
        return View(model);
    }
}
