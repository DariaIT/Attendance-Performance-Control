using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Attendance_Performance_Control.Data;
using Attendance_Performance_Control.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading;

namespace Attendance_Performance_Control.Controllers
{
    public class RecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecordsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }



        // Normal Get: Timer is could be running or not (totalsecond = 0 or Nºseconds)
        //search parameter
        //sort parameter

        [HttpGet]
        public async Task<IActionResult> Index (string sortOrder, string searchString)
        {
            List<UserRecordViewModel> listOfRecords;
            //sorting by data
            ViewData["DateSortParam"] = String.IsNullOrEmpty(sortOrder) ? "data_asc" : "";
            ViewData["CurrentFilter"] = searchString;

            //return list of records of current user in descending order
            listOfRecords = await CreateUserRecordsModel();

            if (!String.IsNullOrEmpty(searchString))
            {
                listOfRecords = listOfRecords.Where(c=>c.Data.Date.ToString().Contains(searchString)).ToList();
            }

            if (String.Compare(sortOrder, "data_asc")==0)
            {
                //return list of records of current user in ascending order
                listOfRecords = listOfRecords.OrderBy(c => c.Data).ToList();
            }
            

            //find total seconds to show with javascript timer
            //@ 0 or NºSeconds
            @ViewBag.totalSeconds = await GetTotalSecondsOfLastTimeRecord();

            return View(listOfRecords);
        }


        //3 Cases of call of this function
        //1. Start button Click - string start = 1
        //2. Stop button click - string start = 0
        //3. Set Interval Type
        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost(string start, string sortOrder, int intervalId, IntervalTypes? IntervalType)
        {
            List<UserRecordViewModel> listOfRecords;
            //sorting by data
            ViewData["DateSortParam"] = String.IsNullOrEmpty(sortOrder) ? "data_asc" : "";

            //find total seconds to show with javascript timer
            //@ 0 or NºSeconds
            @ViewBag.totalSeconds = await GetTotalSecondsOfLastTimeRecord();

            //if intervalId has value, save intervalType in Interval Description
            if (intervalId!=0 && IntervalType != null)
            {
                var thisInterval = _context.IntervalRecords.Where(c => c.Id == intervalId).FirstOrDefault();
                thisInterval.IntervalType = IntervalType;
                await _context.SaveChangesAsync();
            }

            //if start is true
            if (String.Compare(start, "1") == 0)
            {
                Start();
                @ViewBag.totalSeconds = 1;
            }
            //if stop is true
            else if (String.Compare(start, "0") == 0)
            {
                Stop();
                @ViewBag.totalSeconds = 0;
            }

            switch (sortOrder)
            {
                case "data_asc":
                    //return list of records of current user in ascending order
                    listOfRecords = await CreateUserRecordsModel();
                    listOfRecords = listOfRecords.OrderBy(c => c.Data).ToList();
                    break;
                default:
                    //return list of records of current user in descending order
                    listOfRecords = await CreateUserRecordsModel();
                    break;
            }


            return View(listOfRecords);
        }

        private async Task<double> GetTotalSecondsOfLastTimeRecord()
        {
            double totalSecondsUntilNow = 0;
            //current user
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            //dayRecord of today of current user
            var dayRecordToday = _context.DayRecords.Where(c => c.Data.Date == DateTime.Now.Date)
                .FirstOrDefault(c => c.UserId == currentUser.Id);

            //if DayRecord of today already exist
            if (dayRecordToday != null)
            {
                //TimeRecord last
                var lastTimeRecordTodayList = await
                    _context.TimeRecords.Where(c => c.DayRecordId == dayRecordToday.Id).ToListAsync();

                var lastTimeRecordToday = lastTimeRecordTodayList.Last();

                //if EndTime of last TimeRecord is null, it means that TimeRecord is still running 
                if (lastTimeRecordToday.EndTime == null)
                {
                    var totalSeconds = DateTime.Now - lastTimeRecordToday.StartTime;
                    totalSecondsUntilNow = Math.Floor(totalSeconds.TotalSeconds);
                }

            }

            return totalSecondsUntilNow;
        }


        public async Task<List<UserRecordViewModel>> CreateUserRecordsModel()
        {
            //current user
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            //Initialize List of UserRecordViewModel
            var listOfUserRecordViewModel = new List<UserRecordViewModel>();

            //Get All Records of current user
            var recordsListOfCurrentUser =
                await _context.DayRecords.Where(c => c.UserId == currentUser.Id).OrderByDescending(c => c.Data).ToListAsync();

            //create list of UserRecordViewModel
            foreach (var record in recordsListOfCurrentUser)
            {
                //create and initialize UserRecordViewModel
                var userRecord = new UserRecordViewModel()
                {
                    Data = record.Data,
                    DayStartTime = await GetDateStartTime(record.Id),
                    DayEndTime = await GetDateEndTime(record.Id),
                    IntervalsList = await GetIntervalsList(record.Id)
                };
                //if do not exist yet DayEndTime - do not show in table
                if (userRecord.DayEndTime != null)
                {
                    listOfUserRecordViewModel.Add(userRecord);
                }

            }

            //return list of UserRecordViewModel
            return listOfUserRecordViewModel;
        }

        private async Task<List<IntervalRecord>> GetIntervalsList(int dayRecordId)
        {
            //Get All Intervals in db with specific RecordId
            var listOfIntervals = await _context.IntervalRecords.Where(c => c.DayRecordId == dayRecordId).ToListAsync();

            return listOfIntervals;
        }

        private async Task<DateTime> GetDateStartTime(int dayRecordId)
        {
            //Get All TimeRecords in db with specific RecordId
            var listOfTimeRecords = await _context.TimeRecords.Where(c => c.DayRecordId == dayRecordId).OrderBy(c => c.StartTime).ToListAsync();
            var dateStartTime = listOfTimeRecords.First().StartTime;

            return dateStartTime;
        }

        private async Task<DateTime?> GetDateEndTime(int dayRecordId)
        {
            DateTime? dateEndTime = null;

            //Get All TimeRecords in db with specific DayRecordId
            var listOfTimeRecords = await _context.TimeRecords.Where(c => c.DayRecordId == dayRecordId).OrderBy(c => c.StartTime).ToListAsync();

            //if it is more then one TimeRecords and the last TimeRecord is still running (EndTime is null)
            //return EndTime of penultimate TimeRecord
            if (listOfTimeRecords.Count > 1 && listOfTimeRecords.Last().EndTime == null)
            {
                dateEndTime = listOfTimeRecords.Last().StartTime;
            }
            //if EndTime of last TimeRecord not null - all TimeRecords closed - return EndTime
            else if (listOfTimeRecords.Last().EndTime != null)
            {
                dateEndTime = listOfTimeRecords.Last().EndTime;
            }

            return dateEndTime;
        }

        //function that take action after Start Button Click
        [HttpPost]
        public void Start()
        {
            //current user
            var currentUser = _userManager.GetUserAsync(HttpContext.User).Result;

            //check if DayRecord exist with Date.Today
            var dayToday = _context.DayRecords
                .Where(c => c.Data.Date == DateTime.Now.Date).FirstOrDefault(c => c.UserId == currentUser.Id);

            //if not exist DayRecord for today - create new
            if (dayToday == null)
            {
                DayRecord todayRecord = new DayRecord()
                {
                    Data = DateTime.Now,
                    User = currentUser,
                    UserId = currentUser.Id
                };

                _context.DayRecords.Add(todayRecord);
                _context.SaveChanges();
                //create TimeRecord

                TimeRecord timeRecordNew = new TimeRecord()
                {
                    StartTime = todayRecord.Data,
                    DayRecord = todayRecord,
                    DayRecordId = todayRecord.Id
                };

                _context.TimeRecords.Add(timeRecordNew);
                _context.SaveChanges();
            }
            else
            {
                //if DayRecord already exist in db - create new TimeRecord

                TimeRecord timeRecordNew = new TimeRecord()
                {
                    StartTime = DateTime.Now,
                    DayRecord = dayToday,
                    DayRecordId = dayToday.Id
                };

                _context.TimeRecords.Add(timeRecordNew);
                _context.SaveChanges();

                //DayRecord and at least one TimeRecord already exist in db,
                //then on Start button click function create another TimeRecord, it means that we have an Interval
                //create Interval

                var timeRecordsTodayList = _context.TimeRecords.Where(c => c.DayRecordId == dayToday.Id)
                    .OrderBy(c => c.StartTime).ToList();
                //Take penultimate element
                var lastTimeRecordsToday = timeRecordsTodayList[timeRecordsTodayList.Count - 2];

                var intervalNew = new IntervalRecord()
                {
                    StartTime = (DateTime)lastTimeRecordsToday.EndTime,
                    EndTime = (DateTime)timeRecordNew.StartTime,
                    DayRecord = dayToday,
                    DayRecordId = dayToday.Id
                };

                _context.IntervalRecords.Add(intervalNew);
                _context.SaveChanges();

            }

        }



        //function that take action after Stop Button Click
        //called by javascript
        //find last TimeRecord and save EndDate in db
        [HttpPost]
        public void Stop()
        {
            //current user
            var currentUser = _userManager.GetUserAsync(HttpContext.User).Result;

            //find today DayRecord
            var dayToday = _context.DayRecords
                .Where(c => c.Data.Date == DateTime.Now.Date).FirstOrDefault(c => c.UserId == currentUser.Id);

            //find last TimeRecord of today date
            var timeRecordToClose = _context.TimeRecords.Where(c => c.DayRecordId == dayToday.Id)
                .OrderBy(c => c.StartTime).Last();

            timeRecordToClose.EndTime = DateTime.Now;
            _context.SaveChanges();
        }







        // GET: Records/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dayRecord = await _context.DayRecords
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dayRecord == null)
            {
                return NotFound();
            }

            return View(dayRecord);
        }

        // GET: Records/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Records/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Data,UserId")] DayRecord dayRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dayRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", dayRecord.UserId);
            return View(dayRecord);
        }

        // GET: Records/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dayRecord = await _context.DayRecords.FindAsync(id);
            if (dayRecord == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", dayRecord.UserId);
            return View(dayRecord);
        }

        // POST: Records/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Data,UserId")] DayRecord dayRecord)
        {
            if (id != dayRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dayRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DayRecordExists(dayRecord.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", dayRecord.UserId);
            return View(dayRecord);
        }

        // GET: Records/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dayRecord = await _context.DayRecords
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dayRecord == null)
            {
                return NotFound();
            }

            return View(dayRecord);
        }

        // POST: Records/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dayRecord = await _context.DayRecords.FindAsync(id);
            _context.DayRecords.Remove(dayRecord);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DayRecordExists(int id)
        {
            return _context.DayRecords.Any(e => e.Id == id);
        }
    }
}
