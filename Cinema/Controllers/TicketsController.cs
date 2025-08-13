using Cinema.Models;
using CinemaProjections.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Cinema.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tickets/Create?projectionId=5
        [Authorize]
        public async Task<IActionResult> Create(int projectionId)
        {
            var projection = await _context.Projections
                .Include(p => p.Hall)
                .Include(p => p.Movie)
                .FirstOrDefaultAsync(p => p.Id == projectionId);

            if (projection == null)
                return NotFound();

            ViewBag.Projection = projection;

            var takenSeats = await _context.Tickets
                .Where(t => t.ProjectionId == projectionId)
                .Select(t => new { t.SeatRow, t.SeatColumn })
                .ToListAsync();

            ViewBag.TakenSeats = takenSeats;

            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = false;
            int userTicketsCount = 0;

            if (user != null)
            {
                isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                if (!isAdmin)
                {
                    userTicketsCount = await _context.Tickets
                    .CountAsync(t => t.ProjectionId == projectionId && t.UserId == user.Id);
                }
            }

            ViewBag.UserTicketsCount = userTicketsCount;
            ViewBag.IsAdmin = isAdmin;

            return View(new Ticket { ProjectionId = projectionId });
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            var projection = await _context.Projections
                .Include(p => p.Hall)
                .FirstOrDefaultAsync(p => p.Id == ticket.ProjectionId);

            if (projection == null)
                return NotFound();

            // Проверка колко билета вече е резервирал текущият потребител за тази прожекция
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = false;
            int userTicketsCount = 0;

            if (user != null)
            {
                isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                if (!isAdmin)
                {
                    userTicketsCount = await _context.Tickets
                .CountAsync(t => t.ProjectionId == ticket.ProjectionId && t.UserId == user.Id);

                    // Проверка дали потребителят е резервирал повече от 3 билета за тази прожекция
                    if (userTicketsCount >= 3)
                    {
                        ModelState.AddModelError("", "You cannot reserve more than 3 tickets for this projection.");
                        ViewBag.Projection = projection;
                        ViewBag.TakenSeats = await _context.Tickets
                            .Where(t => t.ProjectionId == ticket.ProjectionId)
                            .Select(t => new { t.SeatRow, t.SeatColumn })
                            .ToListAsync();
                        ViewBag.UserTicketsCount = userTicketsCount;
                        ViewBag.IsAdmin = isAdmin;
                        return View(ticket);
                    }
                }
            }

            bool isTaken = await _context.Tickets.AnyAsync(t =>
                t.ProjectionId == ticket.ProjectionId &&
                t.SeatRow == ticket.SeatRow &&
                t.SeatColumn == ticket.SeatColumn);

            if (isTaken)
            {
                ModelState.AddModelError("", "This seat is already taken.");
                ViewBag.Projection = projection;
                ViewBag.TakenSeats = await _context.Tickets
                    .Where(t => t.ProjectionId == ticket.ProjectionId)
                    .Select(t => new { t.SeatRow, t.SeatColumn })
                    .ToListAsync();
                ViewBag.UserTicketsCount = userTicketsCount;
                ViewBag.IsAdmin = isAdmin;

                return View(ticket);
            }

            if (ticket.SeatRow < 1 || ticket.SeatRow > projection.Hall.Rows || ticket.SeatColumn < 1 || ticket.SeatColumn > projection.Hall.Columns)
            {
                ModelState.AddModelError("", "This seat does not exist.");
                ViewBag.Projection = projection;
                ViewBag.TakenSeats = await _context.Tickets
                    .Where(t => t.ProjectionId == ticket.ProjectionId)
                    .Select(t => new { t.SeatRow, t.SeatColumn })
                    .ToListAsync();
                ViewBag.UserTicketsCount = userTicketsCount;
                ViewBag.IsAdmin = isAdmin;

                return View(ticket);
            }


            // Взимаме текущия логнат потребител и добавяме името му
            ticket.CustomerName = user.FullName;
            ticket.UserId = user.Id;
            ticket.PurchaseTime = DateTime.UtcNow;

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
