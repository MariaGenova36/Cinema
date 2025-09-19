using Cinema.Models;
using CinemaProjections.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager=userManager;
            _roleManager=roleManager;
        }

        public async Task<IActionResult> Reservations(string searchTerm, string sortOrder)
        {
            var tickets = _context.Tickets
                .Include(t => t.Projection).ThenInclude(p => p.Movie)
                .Include(t => t.Projection).ThenInclude(p => p.Hall)
                .Include(t => t.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                tickets = tickets.Where(t =>
                    t.CustomerName.Contains(searchTerm) ||
                    t.Projection.Movie.Title.Contains(searchTerm));
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentSort = sortOrder;

            // Създаваме ViewBag стойности за сортиране на всяка колона (сменя възходящ/низходящ)
            ViewBag.UserSortParam = sortOrder == "user_asc" ? "user_desc" : "user_asc";
            ViewBag.MovieSortParam = sortOrder == "movie_asc" ? "movie_desc" : "movie_asc";
            ViewBag.DateSortParam = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewBag.HallSortParam = sortOrder == "hall_asc" ? "hall_desc" : "hall_asc";
            ViewBag.SeatSortParam = sortOrder == "seat_asc" ? "seat_desc" : "seat_asc";
            ViewBag.PurchasedSortParam = sortOrder == "purchased_asc" ? "purchased_desc" : "purchased_asc";
            ViewBag.TicketSortParam = sortOrder == "ticket_asc" ? "ticket_desc" : "ticket_asc";
            ViewBag.IsPaidSortParam = sortOrder == "paid_asc" ? "paid_desc" : "paid_asc";

            // Сортиране според избраната колона
            tickets = sortOrder switch
            {
                "user_desc" => tickets.OrderByDescending(t => t.CustomerName),
                "user_asc" => tickets.OrderBy(t => t.CustomerName),

                "movie_desc" => tickets.OrderByDescending(t => t.Projection.Movie.Title),
                "movie_asc" => tickets.OrderBy(t => t.Projection.Movie.Title),

                "date_desc" => tickets.OrderByDescending(t => t.Projection.ProjectionTime),
                "date_asc" => tickets.OrderBy(t => t.Projection.ProjectionTime),

                "hall_desc" => tickets.OrderByDescending(t => t.Projection.Hall.Name),
                "hall_asc" => tickets.OrderBy(t => t.Projection.Hall.Name),

                "seat_desc" => tickets.OrderByDescending(t => t.SeatRow).ThenByDescending(t => t.SeatColumn),
                "seat_asc" => tickets.OrderBy(t => t.SeatRow).ThenBy(t => t.SeatColumn),

                "purchased_desc" => tickets.OrderByDescending(t => t.PurchaseTime),
                "purchased_asc" => tickets.OrderBy(t => t.PurchaseTime),

                "ticket_desc" => tickets.OrderByDescending(t => t.Price).ThenByDescending(t => t.TicketType),
                "ticket_asc" => tickets.OrderBy(t => t.Price).ThenBy(t => t.TicketType),

                "paid_desc" => tickets.OrderByDescending(t => t.IsPaid),
                "paid_asc" => tickets.OrderBy(t => t.IsPaid),

                _ => tickets.OrderBy(t => t.Projection.ProjectionTime)
            };

            return View(await tickets.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound();

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction("Reservations");
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<ProfileViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new ProfileViewModel
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    Id = user.Id,
                    Roles = roles.ToList()
                });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);

            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction("Users");
        }

    }
}