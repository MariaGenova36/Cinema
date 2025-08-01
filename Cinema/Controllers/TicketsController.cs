using Cinema.Models;
using CinemaProjections.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class TicketsController : Controller
{
    private readonly ApplicationDbContext _context;
    public TicketsController(ApplicationDbContext context) => _context = context;

    // GET: /Tickets/Create?projectionId=5
    public async Task<IActionResult> Create(int projectionId)
    {
        var projection = await _context.Projections
            .Include(p => p.Movie)
            .Include(p => p.Hall)
            .FirstOrDefaultAsync(p => p.Id == projectionId);

        if (projection == null) return NotFound();
        if (projection.AvailableSeats <= 0)
            return BadRequest("No seats available for this projection.");

        ViewBag.Projection = projection;
        return View(new Ticket { ProjectionId = projectionId });
    }

    // POST: /Tickets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Ticket ticket)
    {
        var projection = await _context.Projections
            .FirstOrDefaultAsync(p => p.Id == ticket.ProjectionId);

        if (projection == null) return NotFound();
        if (projection.AvailableSeats <= 0)
        {
            ModelState.AddModelError("", "No seats available.");
            ViewBag.Projection = projection;
            return View(ticket);
        }

        // запазваме транзакционно (лесен вариант)
        projection.AvailableSeats -= 1;
        _context.Tickets.Add(ticket);
        _context.Projections.Update(projection);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Projections", new { id = ticket.ProjectionId });
    }
}
