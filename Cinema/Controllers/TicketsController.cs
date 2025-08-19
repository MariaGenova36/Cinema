using Cinema.Models;
using CinemaProjections.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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

            // типовете билети и множители
            ViewBag.TicketTypes = new List<dynamic>
    {
        new { Name = "Редовен", Multiplier = 1.0m },
        new { Name = "Дете", Multiplier = 0.5m },
        new { Name = "Студент", Multiplier = 0.75m }
    };

            // добавяме базова цена по подразбиране за "Редовен" билет
            ViewBag.BaseTicketPrice = projection.TicketPrice;
            var defaultTicket = new Ticket
            {
                ProjectionId = projectionId,
                TicketType = "Редовен",
                Price = projection.TicketPrice // редовна цена по подразбиране
            };

            return View(defaultTicket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(int ProjectionId, string SeatRows, string SeatCols, string TicketType, string TicketMultiplier)
        {
            var projection = await _context.Projections
                .Include(p => p.Hall)
                .Include(p => p.Movie)
                .FirstOrDefaultAsync(p => p.Id == ProjectionId);

            if (projection == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = false;
            int userTicketsCount = 0;

            if (user != null)
            {
                isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (!isAdmin)
                {
                    userTicketsCount = await _context.Tickets
                        .CountAsync(t => t.ProjectionId == ProjectionId && t.UserId == user.Id);
                }
            }

            var rows = JsonConvert.DeserializeObject<List<int>>(SeatRows);
            var cols = JsonConvert.DeserializeObject<List<int>>(SeatCols);

            if (rows.Count != cols.Count || rows.Count == 0)
            {
                ModelState.AddModelError("", "Invalid seat selection.");
                await LoadViewBags(projection, ProjectionId, userTicketsCount, isAdmin);
                return View(new Ticket { ProjectionId = ProjectionId });
            }

            if (!isAdmin && (userTicketsCount + rows.Count) > 3)
            {
                ModelState.AddModelError("", "You cannot reserve more than 3 tickets for this projection.");
                await LoadViewBags(projection, ProjectionId, userTicketsCount, isAdmin);
                return View(new Ticket { ProjectionId = ProjectionId });
            }

            // Парсваме multiplier с InvariantCulture
            var multiplier = decimal.Parse(TicketMultiplier, CultureInfo.InvariantCulture);

            for (int i = 0; i < rows.Count; i++)
            {
                int row = rows[i];
                int col = cols[i];

                bool isTaken = await _context.Tickets.AnyAsync(t =>
                    t.ProjectionId == ProjectionId &&
                    t.SeatRow == row &&
                    t.SeatColumn == col);

                if (isTaken)
                {
                    ModelState.AddModelError("", $"Seat {row}-{col} is already taken.");
                    await LoadViewBags(projection, ProjectionId, userTicketsCount, isAdmin);
                    return View(new Ticket { ProjectionId = ProjectionId });
                }

                var ticket = new Ticket
                {
                    ProjectionId = ProjectionId,
                    SeatRow = row,
                    SeatColumn = col,
                    CustomerName = user.FullName,
                    UserId = user.Id,
                    PurchaseTime = DateTime.UtcNow,
                    TicketType = TicketType,
                    Price = projection.TicketPrice * multiplier
                };

                _context.Tickets.Add(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Success");
        }

        private async Task LoadViewBags(Projection projection, int projectionId, int userTicketsCount, bool isAdmin)
        {
            ViewBag.Projection = projection;
            ViewBag.TakenSeats = await _context.Tickets
                .Where(t => t.ProjectionId == projectionId)
                .Select(t => new { t.SeatRow, t.SeatColumn })
                .ToListAsync();
            ViewBag.UserTicketsCount = userTicketsCount;
            ViewBag.IsAdmin = isAdmin;
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
