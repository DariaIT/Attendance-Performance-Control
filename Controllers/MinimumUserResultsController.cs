using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Attendance_Performance_Control.Data;
using Attendance_Performance_Control.Models;

namespace Attendance_Performance_Control.Controllers
{
    public class MinimumUserResultsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MinimumUserResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MinimumUserResults
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.MinimumUserResults.Include(m => m.User);
            return View(await applicationDbContext.ToListAsync());
        }

       

        // GET: MinimumUserResults/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var minimumUserResults = await _context.MinimumUserResults.FindAsync(id);
            if (minimumUserResults == null)
            {
                return NotFound();
            }

            MinimumUserResultsEditViewModel newMinResult = new MinimumUserResultsEditViewModel()
            {
                Id = minimumUserResults.Id,
                MinimumAuditorias = minimumUserResults.MinimumAuditorias.ToString(),
                MinimumConsultas = minimumUserResults.MinimumConsultas.ToString(),
                MinimumRelatorios = minimumUserResults.MinimumRelatorios.ToString(),
                UserId = minimumUserResults.UserId
            };

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", minimumUserResults.UserId);
            return View(newMinResult);
        }

        // POST: MinimumUserResults/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MinimumAuditorias,MinimumConsultas,MinimumRelatorios,UserId")] MinimumUserResultsEditViewModel newMinimumUserResults)
        {
            if (id != newMinimumUserResults.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var newMinResultsForUpdate = _context.MinimumUserResults.Find(newMinimumUserResults.Id);
                    newMinResultsForUpdate.MinimumAuditorias = Int32.Parse(newMinimumUserResults.MinimumAuditorias);
                    newMinResultsForUpdate.MinimumConsultas = Int32.Parse(newMinimumUserResults.MinimumConsultas);
                    newMinResultsForUpdate.MinimumRelatorios = Int32.Parse(newMinimumUserResults.MinimumRelatorios);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MinimumUserResultsExists(newMinimumUserResults.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "O resultado está modificado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", newMinimumUserResults.UserId);
            return View(newMinimumUserResults);
        }


        private bool MinimumUserResultsExists(int id)
        {
            return _context.MinimumUserResults.Any(e => e.Id == id);
        }
    }
}
