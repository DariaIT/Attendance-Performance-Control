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
    public class BankHolidaysController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BankHolidaysController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BankHolidays
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.BankHolidays.Include(b => b.BankHolidaysType);
            return View(await applicationDbContext.ToListAsync());
        }

       

        // GET: BankHolidays/Create
        public IActionResult Create()
        {
            ViewData["BankHolidaysTypeId"] = new SelectList(_context.BankHolidaysType, "Id", "BankHolidayTypeName");
            return View();
        }

        // POST: BankHolidays/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Data,BankHolidaysTypeId")] BankHoliday bankHoliday)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bankHoliday);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Obrigada. O feriado está adicionado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["BankHolidaysTypeId"] = new SelectList(_context.BankHolidaysType, "Id", "BankHolidayTypeName", bankHoliday.BankHolidaysTypeId);            
            return View(bankHoliday);
        }


        // GET: BankHolidays/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankHoliday = await _context.BankHolidays
                .Include(b => b.BankHolidaysType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bankHoliday == null)
            {
                return NotFound();
            }

            return View(bankHoliday);
        }

        // POST: BankHolidays/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bankHoliday = await _context.BankHolidays.FindAsync(id);
            _context.BankHolidays.Remove(bankHoliday);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Obrigada. O feriado está eliminado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        private bool BankHolidayExists(int id)
        {
            return _context.BankHolidays.Any(e => e.Id == id);
        }
    }
}
