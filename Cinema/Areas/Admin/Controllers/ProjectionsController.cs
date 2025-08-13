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

        // GET: Projections
        public async Task<IActionResult> Index()
        {
            ViewBag.IsAdminView = true; // флаг, че сме в админ панела

            var projections = await _context.Projections
                .Include(p => p.Hall)
                .Include(p => p.Movie)
                .ToListAsync();

            // Създаваме речник за броя заети билети на всяка прожекция
            var ticketsCount = await _context.Tickets
                .GroupBy(t => t.ProjectionId)
                .Select(g => new { ProjectionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ProjectionId, x => x.Count);

            ViewData["TicketsCount"] = ticketsCount;

            return View(projections);
        }

        // GET: Projections/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var projection = await _context.Projections
                .Include(p => p.Movie)
                .Include(p => p.Hall)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projection == null)
                return NotFound();

            ViewBag.IsAdminView = true;

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
                    // Премахваме AvailableSeats, няма нужда да го пълним
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
