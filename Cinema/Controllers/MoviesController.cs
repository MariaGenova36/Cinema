using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cinema.Models;
using CinemaProjections.Data;

namespace Cinema.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string search, int? genreId, string sortOrder)
        {
            ViewBag.IsAdminView = false;

            ViewData["CurrentFilter"] = search;
            ViewData["CurrentGenre"] = genreId;
            ViewData["CurrentSort"] = sortOrder;

            ViewData["TitleSort"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["ReleaseSort"] = sortOrder == "release_asc" ? "release_desc" : "release_asc";
            ViewData["DurationSort"] = sortOrder == "duration_asc" ? "duration_desc" : "duration_asc";

            var movies = _context.Movies.Include(m => m.Genre).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                movies = movies.Where(m => m.Title.Contains(search) || m.Description.Contains(search));
            }

            if (genreId.HasValue)
            {
                movies = movies.Where(m => m.GenreId == genreId);
            }

            switch (sortOrder)
            {
                case "title_desc":
                    movies = movies.OrderByDescending(m => m.Title);
                    break;
                case "release_asc":
                    movies = movies.OrderBy(m => m.ReleaseDate);
                    break;
                case "release_desc":
                    movies = movies.OrderByDescending(m => m.ReleaseDate);
                    break;
                case "duration_asc":
                    movies = movies.OrderBy(m => m.Duration);
                    break;
                case "duration_desc":
                    movies = movies.OrderByDescending(m => m.Duration);
                    break;
                default:
                    movies = movies.OrderBy(m => m.Title);
                    break;
            }

            ViewBag.Genres = new SelectList(await _context.Genres.ToListAsync(), "Id", "Name");

            return View(await movies.ToListAsync());
        }



        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
