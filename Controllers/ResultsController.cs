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
using System.Text.Json;

namespace Attendance_Performance_Control.Controllers
{
    public class ResultsController : BaseController
    {

        public ResultsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
        }

        // GET: Results
        public async Task<IActionResult> Index(string dateRangeSearch, string searchByUser, int? searchByResultType)
        {
            //GetDefaultRangeDataPicker() - get default date for datarangepicker - "This Month" - ex. "01/09/2020 - 30/09/2020"
            //Initialize DataRangePicker: initial default value = "This Month" or date period chosen by client
            dateRangeSearch = String.IsNullOrEmpty(dateRangeSearch) ? GetDefaultRangeDataPicker() : dateRangeSearch;
            ViewData["dateRangeSearch"] = dateRangeSearch;

            //find startDate and EndDate from dateRangeSearch string
            var thisStringArray = dateRangeSearch.Split(" ");
            var startDate = DateTime.Parse(thisStringArray[0]);
            var endDate = DateTime.Parse(thisStringArray[2]);

            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return NotFound();
            }

            var IsAdmin = _userManager.IsInRoleAsync(user, "Admin").Result;

            ViewData["IsAdmin"] = IsAdmin;

            //if Admin and search by user is empty => return all records of all users
            if (IsAdmin && String.IsNullOrEmpty(searchByUser))
                user = null;
            //if Admin and search user has value => return search user results
            else if (IsAdmin && !String.IsNullOrEmpty(searchByUser))
                user = await _userManager.FindByIdAsync(searchByUser);
            //else, if User Role => return results of current user

            //return list of results of given user
            //return list of results in dateRangeSearch period: default "This Month" or defined by client
            //if user=null, return all results of all users (Admin View)
            var listOfResults = await CreateResultsList(startDate, endDate, user, searchByResultType);

            ViewData["ResultTypes"] = new SelectList(_context.ResultTypes,"Id", "ResultTypeName", searchByResultType);
            ViewData["searchByResultType"] = searchByResultType;


            var adminId = GetAdminUserId();
            ViewData["Users"] = new SelectList(_context.Users.Where(c => c.Id != adminId), "Id", "FullName", searchByUser);
            ViewData["searchByUser"] = searchByUser;

            var daysRange = endDate - startDate;
            var days = daysRange.Days;
   
            ViewData["GraficDays"] = days;

            //pass ResultTypeId to function
            // 1 -> Auditorias
            // 2 -> Consultas
            // 3 -> Relatorios
            ViewData["JsonDataForGraficAuditorias"] = GetJsonDataForGraficResults(listOfResults, 1);
            ViewData["JsonDataForGraficConsultas"] = GetJsonDataForGraficResults(listOfResults, 2);
            ViewData["JsonDataForGraficRelatorios"] = GetJsonDataForGraficResults(listOfResults, 3);

            //totals for graf

            //number of working days in period of time - Dias Úteis
            var workingDays = GetNumberOfWorkingDaysInPeriodOfTime(startDate, endDate);
            ViewData["NumberOfWorkingDaysInPeriodOfTime"] = workingDays;

            //send data to view if user is not null
            ViewData["IsSingleUser"] = user == null ? false : true;
            if (user != null)
            {
                //number of days that user registed
                var registDays = _context.DayRecords.Where(c => c.UserId == user.Id && c.Data >= startDate && c.Data <= endDate).Count();
                ViewData["RegistedDaysForPeriodOfTime"] = registDays == null ? 0 : registDays;
                int holidaysDays = _context.UserHolidays.Where(c => c.UserId == user.Id && c.HolidayDay.Date >= startDate.Date &&
                c.HolidayDay.Date <= endDate.Date).Count();
                ViewData["NumberOfUserHolidayDaysInPeriodOfTime"] = holidaysDays == null ? 0 : holidaysDays;
                //calculate number of expected working days
                //if any day of holiday overlap with Bank Holyday ou Saturday or Sunday -> do not subtract from working days
                var expectedWorkingDays = GetNumberOfExpectedWorkingDaysInPeriodOfTime(startDate, endDate, user, workingDays, holidaysDays);
                ViewData["NumberOfExpectedWorkingDaysInPeriodOfTime"] = expectedWorkingDays;

                //get minimum limits defined for this user
                //system create limits automaticamente then Admin make user Confirm (2,2,30)
                //Admin can edit min limits later
                //limits could be zero, for users who do not need submit results
                ViewData["MinLimitAuditorias"] = _context.MinimumUserResults.FirstOrDefault(c => c.UserId == user.Id).MinimumAuditorias;
                ViewData["MinLimitConsultas"] = _context.MinimumUserResults.FirstOrDefault(c => c.UserId == user.Id).MinimumConsultas;
                ViewData["MinLimitRelatorios"] = _context.MinimumUserResults.FirstOrDefault(c => c.UserId == user.Id).MinimumRelatorios;
            }

            //if user != null then listOfResult contain records of this user
            //if user=null then return sum of all results of all users
            //control this in View, show diferent views ofr single or all users
            var auditorias = listOfResults.Where(c => c.ResultTypeId == 1).Sum(c => c.NumberOfResults);
            var consultas = listOfResults.Where(c => c.ResultTypeId == 2).Sum(c => c.NumberOfResults);
            var relatorios = listOfResults.Where(c => c.ResultTypeId == 3).Sum(c => c.NumberOfResults);

            ViewData["TotalAuditorias"] = auditorias == null ? 0 : auditorias;
            ViewData["TotalConsultas"] = consultas == null ? 0 : consultas;
            ViewData["TotalRelatorios"] = relatorios == null ? 0 : relatorios;

            return View(listOfResults);
        }


        public async Task<List<Result>> CreateResultsList(DateTime startDate, DateTime endDate, ApplicationUser user, int? searchByResultType)
        {
            //filter by date period between startDate and EndDate
            var listOfResults = await _context.Results.Where(c => c.Data.Date >= startDate.Date && c.Data.Date <= endDate.Date).ToListAsync();
            //filter by user
            if (user != null)
                listOfResults = listOfResults.Where(c => c.UserId == user.Id).ToList();
            //filter by ResultType
            if (searchByResultType != null)
                listOfResults = listOfResults.Where(c => c.ResultTypeId == searchByResultType).ToList();

            //return list of Results
            return listOfResults;
        }


        // GET: Results/Create
        public IActionResult Create()
        {
            ViewData["ResultTypeId"] = new SelectList(_context.ResultTypes, "Id", "ResultTypeName");
            return View();
        }

        //On creation of Result, function see if the Result with this Date already exists and if true - sume the new entry,
        //otherwise create new entry for the new Result
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Number,ResultTypeId")] ResultRegistrationViewModel newResult)
        {
            if (ModelState.IsValid)
            {
                //if string number to int conversion is successful
                if (Int32.TryParse(newResult.Number, out int number))
                    {
                    //if entry for of this ResultTypeId allready exist for Today -> sume NumberOfResults
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
