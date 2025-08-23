using Cinema.Models;
using Cinema.Services;
using CinemaProjections.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly IEmailSender _emailSender;

        public TicketsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
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
        new { Name = "Regular", Multiplier = 1.0m },
        new { Name = "Kid", Multiplier = 0.5m },
        new { Name = "Student", Multiplier = 0.75m }
    };

            // добавяме базова цена по подразбиране за "Редовен" билет
            ViewBag.BaseTicketPrice = projection.TicketPrice;
            var defaultTicket = new Ticket
            {
                ProjectionId = projectionId,
                TicketType = "Regular",
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult CreateTemp(int ProjectionId, string SeatRows, string SeatCols, string TicketType, string TicketMultiplier)
        {
            var rows = JsonConvert.DeserializeObject<List<int>>(SeatRows);
            var cols = JsonConvert.DeserializeObject<List<int>>(SeatCols);

            TempData["ProjectionId"] = ProjectionId;
            TempData["SeatRows"] = SeatRows;
            TempData["SeatCols"] = SeatCols;
            TempData["TicketType"] = TicketType;
            TempData["TicketMultiplier"] = TicketMultiplier;

            return RedirectToAction("Checkout");
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            if (TempData["ProjectionId"] == null) return RedirectToAction("Index", "Projections");

            int projectionId = int.Parse(TempData["ProjectionId"].ToString()!);
            var projection = await _context.Projections
                .Include(p => p.Movie)
                .Include(p => p.Hall)
                .FirstOrDefaultAsync(p => p.Id == projectionId);

            ViewBag.Projection = projection;
            ViewBag.SeatRows = JsonConvert.DeserializeObject<List<int>>(TempData["SeatRows"].ToString()!);
            ViewBag.SeatCols = JsonConvert.DeserializeObject<List<int>>(TempData["SeatCols"].ToString()!);
            ViewBag.TicketType = TempData["TicketType"]?.ToString();
            ViewBag.TicketMultiplier = TempData["TicketMultiplier"]?.ToString();

            // запазваме TempData за ConfirmCheckout
            TempData.Keep();

            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ConfirmCheckout()
        {
            int projectionId = int.Parse(TempData["ProjectionId"].ToString()!);
            var projection = await _context.Projections
    .Include(p => p.Movie)
    .Include(p => p.Hall)
    .FirstOrDefaultAsync(p => p.Id == projectionId);
            var rows = JsonConvert.DeserializeObject<List<int>>(TempData["SeatRows"].ToString()!);
            var cols = JsonConvert.DeserializeObject<List<int>>(TempData["SeatCols"].ToString()!);
            string ticketType = TempData["TicketType"].ToString();
            var multiplier = decimal.Parse(TempData["TicketMultiplier"].ToString()!, CultureInfo.InvariantCulture);

            var user = await _userManager.GetUserAsync(User);

            for (int i = 0; i < rows.Count; i++)
            {
                var ticket = new Ticket
                {
                    ProjectionId = projectionId,
                    SeatRow = rows[i],
                    SeatColumn = cols[i],
                    CustomerName = user.FullName,
                    UserId = user.Id,
                    TicketType = ticketType,
                    Price = projection.TicketPrice * multiplier,
                    IsPaid = true,
                    PurchaseTime = DateTime.UtcNow
                };

                _context.Tickets.Add(ticket);
            }

            await _context.SaveChangesAsync();

            // Проверка за projection и Movie
            if (projection == null || projection.Movie == null || projection.Hall == null)
            {
                // Не можем да изпратим имейл, просто продължаваме към Success
                return RedirectToAction("Success");
            }

            // Проверка за потребител и имейл
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return RedirectToAction("Success");
            }

            // Изпращане на потвърдителен имейл
            string emailBody = $@"
        <h3>Reservation Confirmed!</h3>
        <p>Movie: {projection.Movie.Title}</p>
        <p>Time: {projection.ProjectionTime:dd.MM.yyyy HH:mm}</p>
        <p>Hall: {projection.Hall.Name}</p>
        <p>Ticket type: {ticketType}</p>
        <p>Seats: {string.Join(", ", rows.Select((r, i) => $"Row {r}, Seat {cols[i]}"))}</p>
        <p>Total: {(projection.TicketPrice * multiplier * rows.Count):F2} лв.</p>
    ";

            await _emailSender.SendEmailAsync(user.Email, "Cinema Reservation Confirmation", emailBody);

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
