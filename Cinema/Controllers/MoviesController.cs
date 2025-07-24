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

        // GET: Movies/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name");
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie)
        {
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);

            if (!ModelState.IsValid)
            {
                // Debug output to console or log file
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                return View(movie);
            }

            // Save image
            if (movie.PosterFile != null && movie.PosterFile.Length > 0)
            {
                var fileName = Path.GetFileName(movie.PosterFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await movie.PosterFile.CopyToAsync(stream);
                }

                movie.PosterUrl = "/images/" + fileName;
            }

            _context.Add(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
         ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,GenreId,Duration,ReleaseDate,PosterFile")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var movieFromDb = await _context.Movies.FindAsync(id);
                    if (movieFromDb == null)
                    {
                        return NotFound();
                    }

                    // Обнови основните свойства
                    movieFromDb.Title = movie.Title;
                    movieFromDb.Description = movie.Description;
                    movieFromDb.GenreId = movie.GenreId;
                    movieFromDb.Duration = movie.Duration;
                    movieFromDb.ReleaseDate = movie.ReleaseDate;

                    // Ако е качен нов файл, запази новата снимка
                    if (movie.PosterFile != null)
                    {
                        var fileName = Path.GetFileName(movie.PosterFile.FileName);
                        var filePath = Path.Combine("wwwroot/images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await movie.PosterFile.CopyToAsync(stream);
                        }

                        movieFromDb.PosterUrl = "/images/" + fileName;
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
            return View(movie);
        }


        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                if (!string.IsNullOrEmpty(movie.PosterUrl))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", movie.PosterUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
