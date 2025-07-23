using Cinema.Models;
using CinemaProjections.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Cinema.Controllers
{
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
            var projections = _context.Projections
                .Include(p => p.Movie)
                .Include(p => p.Hall);

            return View(await projections.ToListAsync());
        }

        // GET: Projections/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var projection = await _context.Projections
                .Include(p => p.Movie)
                .Include(p => p.Hall)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (projection == null) return NotFound();

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
                    projection.AvailableSeats = hall.SeatCount;
                    _context.Add(projection);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

           
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", projection.MovieId);
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name", projection.HallId);

            return View(new Projection());
        }

        [HttpGet]
        public JsonResult GetSeatCount(int hallId)
        {
            var hall = _context.Halls.FirstOrDefault(h => h.Id == hallId);
            int seatCount = hall?.SeatCount ?? 0;
            return Json(new { seatCount });
        }

        // GET: Projections/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var projection = await _context.Projections.FindAsync(id);
            if (projection == null) return NotFound();

            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", projection.MovieId);
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name", projection.HallId);
            return View(projection);
        }

        // POST: Projections/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,HallId,ProjectionTime,TicketPrice,AvailableSeats")] Projection projection)
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
    }
}
