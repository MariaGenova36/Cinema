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

            return View(projection);
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
