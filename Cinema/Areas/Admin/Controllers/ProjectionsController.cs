using Cinema.Models;
using CinemaProjections.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProjectionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Projections/Index
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewBag.IsAdminView = true; // админ панел

            var projectionsQuery = _context.Projections
                .Include(p => p.Hall)
                .Include(p => p.Movie)
                .AsQueryable();

            // Филтриране по заглавие на филма
            if (!string.IsNullOrEmpty(searchString))
            {
                projectionsQuery = projectionsQuery
                    .Where(p => p.Movie.Title.Contains(searchString));
                ViewBag.CurrentFilter = searchString;
            }

            // Сортиране
            projectionsQuery = sortOrder switch
            {
                "title_desc" => projectionsQuery.OrderByDescending(p => p.Movie.Title),
                "title_asc" => projectionsQuery.OrderBy(p => p.Movie.Title),
                "time_desc" => projectionsQuery.OrderByDescending(p => p.ProjectionTime),
                "time_asc" => projectionsQuery.OrderBy(p => p.ProjectionTime),
                _ => projectionsQuery.OrderBy(p => p.ProjectionTime),
            };

            // Опции за drop-down
            ViewBag.SortOptions = new List<SelectListItem>
    {
        new SelectListItem { Value = "time_asc", Text = "Time Ascending" },
        new SelectListItem { Value = "time_desc", Text = "Time Descending" },
        new SelectListItem { Value = "title_asc", Text = "Title A-Z" },
        new SelectListItem { Value = "title_desc", Text = "Title Z-A" }
    };
            ViewBag.CurrentSort = sortOrder;

            var projections = await projectionsQuery.ToListAsync();

            // Брой заети билети
            var ticketsCount = await _context.Tickets
                .GroupBy(t => t.ProjectionId)
                .Select(g => new { ProjectionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ProjectionId, x => x.Count);

            ViewData["TicketsCount"] = ticketsCount;

            return View("Index", projections); // Използва същото View като нормалния потребител
        }

        // GET: Projections/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var projection = await _context.Projections
                .Include(p => p.Movie)
                .Include(p => p.Hall)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projection == null)
            {
                return NotFound();
            }

            // общ брой места
            var totalSeats = projection.Hall.Rows * projection.Hall.Columns;

            // броим вече заетите (продадени) билети
            var takenSeats = await _context.Tickets
                .CountAsync(t => t.ProjectionId == id);

            ViewBag.TotalSeats = totalSeats;
            ViewBag.TakenSeatsCount = takenSeats;

            return View(projection);
        }

        // GET: Projections/Create
        public IActionResult Create()
        {
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title");
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name");
            return View(new Projection());
        }

        // POST: Projections/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Projection projection)
        {
            if (ModelState.IsValid)
            {
                var hall = await _context.Halls.FindAsync(projection.HallId);

                if (hall == null)
                {
                    ModelState.AddModelError("", "Залата не съществува.");
                }
                else
                {
                    _context.Add(projection);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", projection.MovieId);
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name", projection.HallId);

            return View(projection);
        }

        // GET: Projections/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var projection = await _context.Projections
                .Include(p => p.Hall)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projection == null) return NotFound();

            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", projection.MovieId);
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name", projection.HallId);

            return View(projection);
        }

        // POST: Projections/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,HallId,ProjectionTime,TicketPrice")] Projection projection)
        {
            if (id != projection.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projection);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectionExists(projection.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", projection.MovieId);
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name", projection.HallId);
            return View(projection);
        }

        // GET: Projections/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var projection = await _context.Projections
                .Include(p => p.Movie)
                .Include(p => p.Hall)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (projection == null) return NotFound();

            int takenSeatsCount = await _context.Tickets.CountAsync(t => t.ProjectionId == id);

            return View(projection);
        }

        // POST: Projections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var projection = await _context.Projections.FindAsync(id);
            _context.Projections.Remove(projection);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectionExists(int id)
        {
            return _context.Projections.Any(e => e.Id == id);
        }

        //връща редове и колони на залата
        [HttpGet]
        public async Task<JsonResult> GetHallSeats(int hallId)
        {
            var hall = await _context.Halls.FindAsync(hallId);
            if (hall == null)
                return Json(new { rows = 0, columns = 0 });

            return Json(new { rows = hall.Rows, columns = hall.Columns });
        }
    }
}