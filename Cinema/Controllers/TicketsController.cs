using Cinema.Models;
using Cinema.Services;
using CinemaProjections.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
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
            var emailBody = $@"
<p style='font-family: Arial, sans-serif; color: #000000; font-size: 14px;'>Hello, <strong>{user.FullName}</strong>!</p>

<p style='font-family: Arial, sans-serif; color: #000000; font-size: 14px;'>Thank you for booking with <strong>Starluxe Cinema</strong>!</p>

<p style='font-family: Arial, sans-serif; color: #000000; font-size: 14px;'>Here are your reservation details:</p>

<ul style='font-family: Arial, sans-serif; color: #000000; font-size: 14px;'>
    <li><strong>Movie:</strong> {projection.Movie.Title}</li>
    <li><strong>Time:</strong> {projection.ProjectionTime:dd.MM.yyyy HH:mm}</li>
    <li><strong>Hall:</strong> {projection.Hall.Name}</li>
    <li><strong>Ticket type:</strong> {ticketType}</li>
    <li><strong>Seats:</strong> {string.Join(", ", rows.Select((r, i) => $"Row {r}, Seat {cols[i]}"))}</li>
    <li><strong>Total:</strong> {(projection.TicketPrice * multiplier * rows.Count):F2} лв.</li>
</ul>

<p style='font-family: Arial, sans-serif; color: #000000; font-size: 14px;'>Please keep this email for your records.</p>

<p style='font-family: Arial, sans-serif; color: #000000; font-size: 14px;'>Best regards,<br>
<strong>The Starluxe Cinema Team</strong></p>
";

            await _emailSender.SendEmailAsync(user.Email, "Starluxe Cinema Reservation Confirmation", emailBody);

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }

        //HTTP отговор – връща PNG изображение директно на браузъра
        public IActionResult QRCode(int id)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket == null) return NotFound();

            var qrBytes = QRCodeGeneratorBytes(id);
            return File(qrBytes, "image/png");
        }

        public IActionResult DownloadTicketPdf(int id)
        {
            var ticket = _context.Tickets
                .Include(t => t.Projection)
                    .ThenInclude(p => p.Movie)
                .Include(t => t.Projection)
                    .ThenInclude(p => p.Hall)
                .Include(t => t.User)
                .FirstOrDefault(t => t.Id == id);

            if (ticket == null)
                return NotFound();

            var qrBytes = QRCodeGeneratorBytes(ticket.Id);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // 72 точки на инч
                    float width = 6f * 72;   // 6 inches
                    float height = 3f * 72;  // 3 inches
                    page.Size(width, height);
                    page.Margin(10);

                    // Рамка за изрязване
                    page.Content().Padding(5).Border(0.5f).BorderColor("#808080").Row(row =>
                    {
                        // Лява част (информация)
                        row.RelativeItem().Padding(5).Column(col =>
                        {
                            col.Spacing(8);
                            col.Item().Text("STARLUXE CINEMA TICKET").FontSize(14).Bold().AlignCenter();
                            col.Item().LineHorizontal(1).LineColor(Colors.Black);
                            col.Item().Text($"Movie: {ticket.Projection.Movie.Title}").FontSize(11).Bold();
                            col.Item().Text($"Date & Time: {ticket.Projection.ProjectionTime:dd.MM.yyyy HH:mm}").FontSize(10);
                            col.Item().Text($"Hall: {ticket.Projection.Hall.Name}").FontSize(10);
                            col.Item().Text($"Seat: Row {ticket.SeatRow}, Seat {ticket.SeatColumn}").FontSize(10);
                            col.Item().LineHorizontal(0.5f).LineColor(Colors.Black);
                            col.Item().Text($"Ticket Type: {ticket.TicketType}").FontSize(9).Italic();
                            col.Item().Text($"Price: {ticket.Price:C}").FontSize(10).Bold();
                            col.Item().Text($"Customer: {ticket.User?.FullName ?? "N/A"}").FontSize(9);
                        });

                        // Дясна част (QR код)
                        row.ConstantItem(120).Padding(6).BorderLeft(0.5f).BorderColor("#808080").AlignMiddle().Column(qrCol =>
                        {
                            qrCol.Spacing(6);

                            // Горна перфорация
                            qrCol.Item().AlignCenter().Row(r =>
                            {
                                for (int i = 0; i < 15; i++)
                                {
                                    r.ConstantItem(4).Height(4).Background(Colors.Black);
                                    r.ConstantItem(2);
                                }
                            });

                            qrCol.Item().Text("SCAN HERE").AlignCenter().FontSize(10).Bold();

                            qrCol.Item()
                            .Height(100)
                            .AlignCenter()
                            .AlignMiddle()
                            .Element(qrContainer =>
                            {
                                // Зареждаме изображението от байтов масив
                                var imageDescriptor = qrContainer.Image(qrBytes);
                                // Можем да приложим допълнителни настройки към imageDescriptor
                                imageDescriptor.FitArea();
                                });

                            // Долна перфорация
                            qrCol.Item().AlignCenter().Row(r =>
                            {
                                for (int i = 0; i < 15; i++)
                                {
                                    r.ConstantItem(4).Height(4).Background(Colors.Black);
                                    r.ConstantItem(2);
                                }
                            });

                            // Ticket ID под QR кода
                            qrCol.Item().PaddingTop(4).Text($"Ticket ID: {ticket.Id}")
                                .AlignCenter().FontSize(9).Italic();
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Starluxe Ticket_{ticket.Id}.pdf");
        }

        // Вземане на байтовете на QR кода чрез съществуващия метод;QR кода в PDF, имейл, друг поток, без да се записва файл
        private byte[] QRCodeGeneratorBytes(int id)
        {
            using var qrGenerator = new QRCoder.QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(id.ToString(), QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCoder.PngByteQRCode(qrData);
            return qrCode.GetGraphic(20);
        }
    }
}