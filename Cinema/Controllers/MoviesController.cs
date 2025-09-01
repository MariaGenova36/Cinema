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


            var movies = _context.Movies.Include(m => m.Genre).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                movies = movies.Where(m => m.Title.Contains(search) || m.Description.Contains(search));
            }

            if (genreId.HasValue)
            {
                movies = movies.Where(m => m.GenreId == genreId);
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
