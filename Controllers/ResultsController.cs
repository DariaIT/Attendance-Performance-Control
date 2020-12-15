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
    public class ResultsController : BaseController
    {

        public ResultsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
        }

        // GET: Results
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Results.Include(r => r.Type).Include(r => r.User);
            return View(await applicationDbContext.ToListAsync());
        }


        // GET: Results/Create
        public IActionResult Create()
        {
            ViewData["ResultTypeId"] = new SelectList(_context.ResultTypes, "Id", "ResultTypeName");
            return View();
        }

        // POST: Results/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Number,ResultTypeId")] ResultRegistrationViewModel newResult)
        {
            if (ModelState.IsValid)
            {
                //if string number to int conversion is successful
                if (Int32.TryParse(newResult.Number, out int number))
                    {
                    //if entry for of this ResultTypeId allready exist for Today -> some NumberOfResults
                    var resultForToday = _context.Results.FirstOrDefault(c => c.Data.Date == DateTime.Now.Date &&
                    c.ResultTypeId == newResult.ResultTypeId);
                    if (resultForToday != null)
                    {
                        resultForToday.NumberOfResults += number;
                        await _context.SaveChangesAsync();
                    }
                    //else create new Result entry
                    else
                    {
                        Result newRegist = new Result()
                        {
                            Data = DateTime.Now,
                            NumberOfResults = number,
                            ResultTypeId = newResult.ResultTypeId,
                            UserId = _userManager.GetUserId(User)
                        };
                        _context.Add(newRegist);
                        await _context.SaveChangesAsync();
                    }
                TempData["Success"] = "O resultado está registado com sucesso.";
                return RedirectToAction(nameof(Create));
                }
            }

            TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";

            ViewData["ResultTypeId"] = new SelectList(_context.ResultTypes, "Id", "ResultTypeName", newResult.ResultTypeId);

            return View(newResult);
        }

        // GET: Results/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _context.Results.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            EditResultRegistrationViewModel newResultModel = new EditResultRegistrationViewModel()
            {
                Id = result.Id,
                Number = result.NumberOfResults.ToString(),
                ResultTypeId = result.ResultTypeId
            };

            ViewData["ResultTypeId"] = new SelectList(_context.ResultTypes, "Id", "ResultTypeName", result.ResultTypeId);

            return View(newResultModel);
        }

        // POST: Results/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,ResultTypeId")] EditResultRegistrationViewModel result)
        {
            if (id != result.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var resultForUpdate = _context.Results.Find(result.Id);
                    resultForUpdate.NumberOfResults =Int32.Parse(result.Number);
                    resultForUpdate.ResultTypeId = result.ResultTypeId;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResultExists(result.Id))
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
            ViewData["ResultTypeId"] = new SelectList(_context.ResultTypes, "Id", "ResultTypeName", result.ResultTypeId);

            return View(result);
        }

        // GET: Results/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _context.Results
                .Include(r => r.Type)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        // POST: Results/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _context.Results.FindAsync(id);
            _context.Results.Remove(result);
            await _context.SaveChangesAsync();
            TempData["Success"] = "O resultado está eliminado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        private bool ResultExists(int id)
        {
            return _context.Results.Any(e => e.Id == id);
        }
    }
}
