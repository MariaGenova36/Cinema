using Cinema.Models;
using CinemaProjections.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class TicketsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TicketsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Tickets/Create?projectionId=5
    public async Task<IActionResult> Create(int projectionId)
    {
        var projection = await _context.Projections
            .Include(p => p.Movie)
            .Include(p => p.Hall)
            .FirstOrDefaultAsync(p => p.Id == projectionId);

        if (projection == null) return NotFound();
        if (projection.AvailableSeats <= 0) return BadRequest("No seats available.");

        ViewBag.Projection = projection;
        return View(new Ticket { ProjectionId = projectionId });
    }

    // POST: /Tickets/Create (from form)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Ticket ticket)
    {
        var user = User.Identity?.IsAuthenticated == true
            ? await _userManager.GetUserAsync(User)
            : null;

        var result = await TryCreateTicket(ticket.ProjectionId, ticket.SeatNumber, user?.Id, ticket.CustomerName);
        if (result.success)
        {
            return RedirectToAction("Details", "Projections", new { id = ticket.ProjectionId });
        }

        ModelState.AddModelError("", result.errorMessage ?? "Error booking ticket");
        ViewBag.Projection = await _context.Projections
            .Include(p => p.Movie)
            .Include(p => p.Hall)
            .FirstOrDefaultAsync(p => p.Id == ticket.ProjectionId);
        return View(ticket);
    }

    // POST: /Tickets/BookTicket (auto-book for logged-in users)
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> BookTicket(int projectionId, int seatNumber)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await TryCreateTicket(projectionId, seatNumber, user.Id, null);

        if (result.success)
            return RedirectToAction("Index", "Profile");

        return BadRequest(result.errorMessage);
    }

    // Централизиран метод за създаване на билет
    private async Task<(bool success, string? errorMessage)> TryCreateTicket(int projectionId, int seatNumber, string? userId, string? customerName)
    {
        var projection = await _context.Projections.FirstOrDefaultAsync(p => p.Id == projectionId);
        if (projection == null) return (false, "Projection not found");

        if (projection.AvailableSeats <= 0)
            return (false, "No seats available.");

        var ticket = new Ticket
        {
            ProjectionId = projectionId,
            SeatNumber = seatNumber,
            UserId = userId,
            CustomerName = customerName,
            PurchaseTime = DateTime.UtcNow
        };

        projection.AvailableSeats -= 1;
        _context.Tickets.Add(ticket);
        _context.Projections.Update(projection);
        await _context.SaveChangesAsync();

        return (true, null);
    }
}
