﻿using System;
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
using X.PagedList;
using System.Runtime.InteropServices;

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
        public async Task<IActionResult> Index(string dateSortParam, string dateRangeSearch, int? page)
        {
            List<UserRecordViewModel> listOfRecords;
            //sorting by data
            if (page != null && page > 1)
                ViewData["dateSortParam"] = dateSortParam;
            else
                ViewData["dateSortParam"] = String.IsNullOrEmpty(dateSortParam) ? "data_asc" : "";

            //GetDefaultRangeDataPicker() - get default date for datarangepicker - "This Month" - ex. "01/09/2020 - 30/09/2020"
            //Initialize DataRangePicker: initial default value = "This Month" or date period chosen by client
            dateRangeSearch = String.IsNullOrEmpty(dateRangeSearch) ? GetDefaultRangeDataPicker() : dateRangeSearch;
            ViewData["dateRangeSearch"] = dateRangeSearch;

            //find startDate and EndDate from dateRangeSearch string
            var thisStringArray = dateRangeSearch.Split(" ");
            var startDate = DateTime.Parse(thisStringArray[0]);
            var endDate = DateTime.Parse(thisStringArray[2]);

            //return list of records of current user in descending order
            //return list of records in dateRangeSearch period: default "This Month" or defined by client
            listOfRecords = await CreateUserRecordsModel(startDate, endDate);

            if (String.Compare(dateSortParam, "data_asc") == 0)
            {
                //return list of records of current user in ascending order
                listOfRecords = listOfRecords.OrderBy(c => c.Data).ToList();
            }


            //find total seconds to show with javascript timer
            //@ 0 or NºSeconds
            @ViewBag.totalSeconds = await GetTotalSecondsOfLastTimeRecord();

            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfrecords = listOfRecords.ToPagedList(pageNumber, 10); // will only contain 25 products max because of the pageSize

            return View(onePageOfrecords);

        }


        //3 Cases of call of this function
        //1. Start button Click - string start = 1
        //2. Stop button click - string start = 0
        //3. Set Interval Type
        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost(string start, string dateSortParam, string dateRangeSearch, int intervalId, IntervalTypes? IntervalType, int? page)
        {
            List<UserRecordViewModel> listOfRecords;
            //sorting by data
            if (page != null && page > 1)
                ViewData["dateSortParam"] = dateSortParam;
            else
                ViewData["dateSortParam"] = String.IsNullOrEmpty(dateSortParam) ? "data_asc" : "";

            //GetDefaultRangeDataPicker() - get default date for datarangepicker - "This Month" - ex. "01/09/2020 - 30/09/2020"
            //Initialize DataRangePicker: initial default value = "This Month" or date period chosen by client
            dateRangeSearch = String.IsNullOrEmpty(dateRangeSearch) ? GetDefaultRangeDataPicker() : dateRangeSearch;
            ViewData["dateRangeSearch"] = dateRangeSearch;

            //find startDate and EndDate from dateRangeSearch string
            var thisStringArray = dateRangeSearch.Split(" ");
            var startDate = DateTime.Parse(thisStringArray[0]);
            var endDate = DateTime.Parse(thisStringArray[2]);

            //find total seconds to show with javascript timer
            //@ 0 or NºSeconds
            @ViewBag.totalSeconds = await GetTotalSecondsOfLastTimeRecord();

            //if intervalId has value, save intervalType in Interval Description
            if (intervalId != 0 && IntervalType != null)
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

            //return list of records of current user in descending order
            //return list of records in dateRangeSearch period: default "This Month" or defined by client
            listOfRecords = await CreateUserRecordsModel(startDate, endDate);

            if (String.Compare(dateSortParam, "data_asc") == 0)
            {
                //return list of records of current user in ascending order
                listOfRecords = listOfRecords.OrderBy(c => c.Data).ToList();
            }

            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfrecords = listOfRecords.ToPagedList(pageNumber, 7);

            return View(onePageOfrecords);
        }


        private string GetDefaultRangeDataPicker()
        {
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            return String.Concat(firstDayOfMonth.ToShortDateString(), " - ", lastDayOfMonth.ToShortDateString());
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


        public async Task<List<UserRecordViewModel>> CreateUserRecordsModel(DateTime startDate, DateTime endDate)
        {
            //current user
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            //Initialize List of UserRecordViewModel
            var listOfUserRecordViewModel = new List<UserRecordViewModel>();

            //Get All Records of current user in period "startDate to endDate"
            var recordsListOfCurrentUser =
                await _context.DayRecords.Where(c => c.UserId == currentUser.Id && c.Data.Date >= startDate && c.Data.Date <= endDate).OrderByDescending(c => c.Data).ToListAsync();

            //create list of UserRecordViewModel
            foreach (var record in recordsListOfCurrentUser)
            {
                //create and initialize UserRecordViewModel
                var userRecord = new UserRecordViewModel()
                {
                    Data = record.Data,
                    DayStartTime = await GetDateStartTime(record.Id),
                    DayEndTime = await GetDateEndTime(record.Id),
                    IntervalsList = await GetIntervalsList(record.Id),
                    StartDayDelayExplanation = record.StartDayDelayExplanation,
                    EndDayDelayExplanation = record.EndDayDelayExplanation
                };

                //get sum of all intervals
                TimeSpan intervalSum = new TimeSpan();

                foreach (var interval in userRecord.IntervalsList)
                {
                    intervalSum += interval.EndTime - interval.StartTime;
                }

                //if do not exist yet DayEndTime - do not show in table
                if (userRecord.DayEndTime != null)
                {
                    //Add TotalHoursPorDay
                    var TotalHoursPorDay = userRecord.DayEndTime - userRecord.DayStartTime - intervalSum;
                    userRecord.TotalHoursPorDay = TotalHoursPorDay.Value.ToString("hh\\:mm\\:ss");
                    //Add Delay flags values

                    //if Today
                    if (userRecord.Data.Date == DateTime.Now.Date)
                    {
                        userRecord.StartDayDelayFlag = userRecord.DayStartTime.TimeOfDay > currentUser.StartWorkTime.Value.AddMinutes(5).TimeOfDay ? true : false;
                        userRecord.EndDayDelayFlag = userRecord.DayEndTime.Value.AddMinutes(5).TimeOfDay < currentUser.EndWorkTime.Value.TimeOfDay && DateTime.Now.TimeOfDay >= currentUser.EndWorkTime.Value.TimeOfDay ? true : false;
                    }
                    //if more then one day - show explanation and edit button
                    else
                    {
                        userRecord.StartDayDelayFlag = userRecord.DayStartTime.TimeOfDay > currentUser.StartWorkTime.Value.AddMinutes(5).TimeOfDay ? true : false;
                        userRecord.EndDayDelayFlag = userRecord.DayEndTime.Value.AddMinutes(5).TimeOfDay < currentUser.EndWorkTime.Value.TimeOfDay ? true : false;
                    }
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


        public IActionResult DelayExpl(string data, string StartDayDelayFlag, string EndDayDelayFlag)
        {
            if (String.IsNullOrEmpty(data))
            {
                TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";
                return RedirectToAction(nameof(Index));
            }

            var Data = DateTime.Parse(data);
            //get DayRecord by Data
            var thisDateRecord = _context.DayRecords.FirstOrDefault(c => c.Data.Date == Data.Date);

            if(!String.IsNullOrEmpty(StartDayDelayFlag) && thisDateRecord !=null && !String.IsNullOrEmpty(thisDateRecord.StartDayDelayExplanation))
                ViewData["ExplText"] = thisDateRecord.StartDayDelayExplanation;
            if (!String.IsNullOrEmpty(EndDayDelayFlag) && thisDateRecord != null && !String.IsNullOrEmpty(thisDateRecord.EndDayDelayExplanation))
                ViewData["ExplText"] = thisDateRecord.EndDayDelayExplanation;

            //resend parameters by ViewData
            ViewData["Data"] = data;
            ViewData["StartDayDelayFlag"] = StartDayDelayFlag;
            ViewData["EndDayDelayFlag"] = EndDayDelayFlag;

            return View();
        }


        [HttpPost]
        [ActionName("DelayExpl")]
        public async Task<IActionResult> DelayExplPost(string Data, string ExplText, string StartDayDelayFlag, string EndDayDelayFlag)
        {
            if (String.IsNullOrEmpty(Data))
            {
                TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";
                return RedirectToAction(nameof(Index));
            }

            var data = DateTime.Parse(Data);
            //get DayRecord by Data
            var thisDateRecord = _context.DayRecords.FirstOrDefault(c => c.Data.Date == data.Date);

            if (String.IsNullOrWhiteSpace(ExplText))
            {
                ModelState.AddModelError(String.Empty, "Por favor, adiciona a explicação.");
            }
            else if (ExplText.Length > 50)
            {
                ModelState.AddModelError(String.Empty, "A explicação tem de conter máximo 50 caracteres.");
            }
            else
            {
                try
                {
                    //it is a morning delay
                    if (!String.IsNullOrEmpty(StartDayDelayFlag) && Convert.ToBoolean(StartDayDelayFlag))
                    {
                        thisDateRecord.StartDayDelayExplanation = ExplText;
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "A explicação foi guardado com sucesso.";

                        return RedirectToAction(nameof(Index));
                    }
                    //it is a evening delay
                    else if (!String.IsNullOrEmpty(EndDayDelayFlag) && Convert.ToBoolean(EndDayDelayFlag))
                    {
                        thisDateRecord.EndDayDelayExplanation = ExplText;
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "A explicação foi guardado com sucesso.";

                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception)
                {
                    TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";
                }
            }

            //resend parameters by ViewData
            ViewData["Data"] = Data;
            ViewData["StartDayDelayFlag"] = StartDayDelayFlag;
            ViewData["EndDayDelayFlag"] = EndDayDelayFlag;

            if (!String.IsNullOrEmpty(StartDayDelayFlag) && thisDateRecord != null && !String.IsNullOrEmpty(thisDateRecord.StartDayDelayExplanation))
                ViewData["ExplText"] = thisDateRecord.StartDayDelayExplanation;
            if (!String.IsNullOrEmpty(EndDayDelayFlag) && thisDateRecord != null && !String.IsNullOrEmpty(thisDateRecord.EndDayDelayExplanation))
                ViewData["ExplText"] = thisDateRecord.EndDayDelayExplanation;

            return View();
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
