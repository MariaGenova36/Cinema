using Cinema.Models;
using CinemaProjections.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HallsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HallsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Halls
        public async Task<IActionResult> Index()
        {
            var halls = await _context.Halls.ToListAsync();
            return View(halls);
        }

        // GET: Halls/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var hall = await _context.Halls
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hall == null)
                return NotFound();

            return View(hall);
        }

        // GET: Halls/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Halls/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hall hall)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hall);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); 
            }

            return View(hall);
        }


        // GET: Halls/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var hall = await _context.Halls.FindAsync(id);
            if (hall == null)
                return NotFound();

            return View(hall);
        }

        // POST: Halls/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Rows,Columns")] Hall hall)
        {
            if (id != hall.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hall);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HallExists(hall.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hall);
        }

        // GET: Halls/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var hall = await _context.Halls
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hall == null)
                return NotFound();

            return View(hall);
        }

        // POST: Halls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall != null)
            {
                _context.Halls.Remove(hall);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HallExists(int id)
        {
            return _context.Halls.Any(e => e.Id == id);
        }
    }
}
