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
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        { 
            ViewBag.IsAdminView = false;
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

            // Подготвяме опциите за селекта
            ViewBag.SortOptions = new List<SelectListItem>
    {
        new SelectListItem { Value = "time_asc", Text = "Time Ascending" },
        new SelectListItem { Value = "time_desc", Text = "Time Descending" },
        new SelectListItem { Value = "title_asc", Text = "Title A-Z" },
        new SelectListItem { Value = "title_desc", Text = "Title Z-A" }
    };
            ViewBag.CurrentSort = sortOrder;

            var projections = await projectionsQuery.ToListAsync();

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