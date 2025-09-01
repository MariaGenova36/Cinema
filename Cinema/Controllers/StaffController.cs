using Cinema.Models;
using CinemaProjections.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ValidateTicket()
        {
            // Зареждаме всички вече използвани билети
            var usedTickets = await _context.Tickets
                .Where(t => t.IsUsed)
                .Include(t => t.Projection)
                .ThenInclude(p => p.Movie)
                .Include(t => t.Projection)
                .ThenInclude(p => p.Hall)
                .Include(t => t.User)
                .OrderByDescending(t => t.PurchaseTime)
                .ToListAsync();

            ViewBag.UsedTickets = usedTickets;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ValidateTicket([FromBody] ValidateTicketRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            var ticket = await _context.Tickets
                .Include(t => t.Projection)
                .ThenInclude(p => p.Movie)
                .Include(t => t.Projection)
                .ThenInclude(p => p.Hall)
                .Include(t => t.User) // 🔹 добавено
                .FirstOrDefaultAsync(t => t.Id == request.TicketId);

            if (ticket == null)
                return NotFound("Ticket not found");

            if (!ticket.IsPaid)
                return BadRequest("Ticket not paid");

            if (ticket.IsUsed)
                return BadRequest("Ticket already used");

            ticket.IsUsed = true;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Ticket validated successfully",
                movie = ticket.Projection.Movie.Title,
                hall = ticket.Projection.Hall.Name,
                seat = $"Row {ticket.SeatRow}, Seat {ticket.SeatColumn}",
                user = ticket.User?.UserName // 🔹 за да имаш User и в JS
            });
        }
    }
}
