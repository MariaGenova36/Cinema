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
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return NotFound();

        user.FullName = model.FullName;

        // директно с контекста, за да заобиколим Identity UpdateAsync
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        ViewData["Message"] = "Profile updated successfully.";
        ViewBag.Tickets = await GetUserTicketsAsync(user.Id);

        return View(model);
    }
}
