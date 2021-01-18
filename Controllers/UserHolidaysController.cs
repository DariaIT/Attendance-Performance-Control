using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Attendance_Performance_Control.Data;
using Attendance_Performance_Control.Models;
using Microsoft.AspNetCore.Identity;

namespace Attendance_Performance_Control.Controllers
{
    public class UserHolidaysController : BaseController
    {
        public UserHolidaysController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
        }

        // GET: UserHolidays
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.UserHolidays;
            return View(await applicationDbContext.ToListAsync());
        }

      

        // GET: UserHolidays/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserHolidays/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string dateRangeSearch)
        {
            try
            {
                //current user
                var currentUser = _userManager.GetUserAsync(HttpContext.User).Result;                

                //find startDate and EndDate from dateRangeSearch string
                var thisStringArray = dateRangeSearch.Split(" ");
                var startDate = DateTime.Parse(thisStringArray[0]);
                var endDate = DateTime.Parse(thisStringArray[2]);

                //get all userHolidays entries for current user and between start and end date
                //for do not regist same date twice
                var holidaysOfUser = _context.UserHolidays.Where(c => c.UserId == currentUser.Id
                && c.HolidayDay.Date >= startDate.Date && c.HolidayDay.Date <= endDate.Date).ToList();

                while (startDate<=endDate)
                {
                    //add to db if this date is not existed yet
                    var containsDate = holidaysOfUser.FirstOrDefault(c => c.HolidayDay.Date == startDate.Date);

                    if (containsDate == null)
                    {
                        var newUserHoliday = new UserHoliday()
                        {
                            HolidayDay = startDate,
                            UserId = currentUser.Id
                        };

                        _context.UserHolidays.Add(newUserHoliday);
                        _context.SaveChanges();
                    }

                    startDate = startDate.AddDays(1);
                }
                TempData["Success"] = "Obrigada. Os dias de férias estão adicionados com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(String.Empty, "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.");
            }

            return View();
        }
     

        // GET: UserHolidays/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userHoliday = await _context.UserHolidays
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userHoliday == null)
            {
                return NotFound();
            }

            return View(userHoliday);
        }

        // POST: UserHolidays/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userHoliday = await _context.UserHolidays.FindAsync(id);
            _context.UserHolidays.Remove(userHoliday);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Obrigada. O dia de férias está eliminado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        private bool UserHolidayExists(int id)
        {
            return _context.UserHolidays.Any(e => e.Id == id);
        }
    }
}
