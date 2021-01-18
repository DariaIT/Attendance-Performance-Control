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
using X.PagedList;
using System.Runtime.InteropServices;

namespace Attendance_Performance_Control.Controllers
{
    public class RecordsController : BaseController
    {
        public RecordsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
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
            var onePageOfrecords = listOfRecords.ToPagedList(pageNumber, 10);

            return View(onePageOfrecords);

        }


        //3 Cases of call of this function
        //1. Start button Click - string start = 1
        //2. Stop button click - string start = 0
        //3. Set Interval Type
        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost(string start, string dateSortParam, string dateRangeSearch, int? page)
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

            //if start is true
            if (String.Compare(start, "1") == 0)
            {
                Start();
                @ViewBag.totalSeconds = 1;
                return View("Response");              
            }
            //if stop is true
            else if (String.Compare(start, "0") == 0)
            {
                Stop();
                @ViewBag.totalSeconds = 0;
                return View("Response");
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
                //if User forget to stop timer and db.TimeRecords EndTime is NULL
                //Date < Today
                //Normalize Record: set start and end day = user StartWorkTime and EndWorkTime and StartLunchTime and EndLunchTime

                if (record.Data.Date < DateTime.Now.Date)
                {
                    //check if exist TimeRecord with EndTime = null
                    var IsOneRecordNotClosed = CheckIfOneRecordNotClosed(record.Id);
                    if (IsOneRecordNotClosed)
                        SetNormalizeTimetableInTimerLost(record);
                }

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

                //if do not exist yet DayEndTime - do not show in table
                if (userRecord.DayEndTime != null)
                {
                    //Add TotalHoursPorDay
                    var TotalHoursPorDay = (TimeSpan)GetTotalWorkHoursPorDay(record.Id);
                    userRecord.TotalHoursPorDay = TotalHoursPorDay.ToString("hh\\:mm\\:ss");

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


        //function that take action after Start Button Click
        [HttpPost]
        public void Start()
        {
            //current user
            var currentUser = _userManager.GetUserAsync(HttpContext.User).Result;

            //check if DayRecord exist with Date.Today
            var dayToday = _context.DayRecords
                .Where(c => c.Data.Date == DateTime.Now.Date).FirstOrDefault(c => c.UserId == currentUser.Id);
            try
            {
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
                    //if DayRecord already exist in db

                    // check if already exist open TimeRecord (i.e. 24/12/2020 9:01 NULL)
                    // for defend db from second registration of already registed TimeRecord
                    // Double registration happens on telefone on double click, because system react very slow
                    // if open TimeRecord exists - do nothing, redirect to sucess Response Page
                    // If do not exists - create new TimeRecord and redirect to sucess Response Page
                    var timeRecordsTodayLast = _context.TimeRecords.Where(c => c.DayRecordId == dayToday.Id)
                        .OrderBy(c => c.StartTime).ToList().Last();

                    if (timeRecordsTodayLast.EndTime != null)
                    {

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
            }
            catch(Exception)
            {
                TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";
            }
            TempData["Success"] = "Obrigada. As Horas estão registados com sucesso.";         
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

            try
            {
                //check if last TimeRecord is already closed, i.e. EndTime has value
                //protect from double click on telefone
                // if TimeRecord already closed - do nothing, redirect to sucess Response Page
                // if not closed - close last TimeRecord

                //find last TimeRecord of today date
                var timeRecordToClose = _context.TimeRecords.Where(c => c.DayRecordId == dayToday.Id)
                    .OrderBy(c => c.StartTime).Last();
                if (timeRecordToClose.EndTime == null)
                {
                    timeRecordToClose.EndTime = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception)
            {
                TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";
            }
            TempData["Success"] = "Obrigada. As Horas estão registados com sucesso.";
        }


        public IActionResult DelayExpl(string data, string StartDayDelayFlag, string EndDayDelayFlag, string dateRangeSearch, int? page)
        {
            ViewData["dateRangeSearch"] = dateRangeSearch;
            ViewData["page"] = page;
            string dateSortParam = null;

            if (String.IsNullOrEmpty(data))
            {
                TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";
                return RedirectToAction("Index", new { dateSortParam, dateRangeSearch, page });
            }

            var Data = DateTime.Parse(data);
            //get DayRecord by Data
            var thisDateRecord = _context.DayRecords.FirstOrDefault(c => c.Data.Date == Data.Date);

            if (thisDateRecord != null)
            {
                if (!String.IsNullOrEmpty(StartDayDelayFlag) && !String.IsNullOrEmpty(thisDateRecord.StartDayDelayExplanation))
                    ViewData["ExplText"] = thisDateRecord.StartDayDelayExplanation;
                if (!String.IsNullOrEmpty(EndDayDelayFlag) && !String.IsNullOrEmpty(thisDateRecord.EndDayDelayExplanation))
                    ViewData["ExplText"] = thisDateRecord.EndDayDelayExplanation;
            }

            //resend parameters by ViewData
            ViewData["Data"] = data;
            ViewData["StartDayDelayFlag"] = StartDayDelayFlag;
            ViewData["EndDayDelayFlag"] = EndDayDelayFlag;

            return View();
        }


        [HttpPost]
        [ActionName("DelayExpl")]
        public IActionResult DelayExplPost(string Data, string ExplText, string StartDayDelayFlag, string EndDayDelayFlag, string dateRangeSearch, int? page)
        {
            ViewData["dateRangeSearch"] = dateRangeSearch;
            ViewData["page"] = page;
            string dateSortParam = null;

            if (String.IsNullOrEmpty(Data))
            {
                TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";
                return RedirectToAction("Index", new { dateSortParam, dateRangeSearch, page });
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
                        _context.SaveChanges();

                        TempData["Success"] = "A explicação foi guardado com sucesso.";

                        return RedirectToAction("Index", new { dateSortParam, dateRangeSearch, page });
                    }
                    //it is a evening delay
                    else if (!String.IsNullOrEmpty(EndDayDelayFlag) && Convert.ToBoolean(EndDayDelayFlag))
                    {
                        thisDateRecord.EndDayDelayExplanation = ExplText;
                        _context.SaveChanges();

                        TempData["Success"] = "A explicação foi guardado com sucesso.";

                        return RedirectToAction("Index", new { dateSortParam, dateRangeSearch, page });
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

            if (thisDateRecord != null)
            {
                if (!String.IsNullOrEmpty(StartDayDelayFlag) && !String.IsNullOrEmpty(thisDateRecord.StartDayDelayExplanation))
                    ViewData["ExplText"] = thisDateRecord.StartDayDelayExplanation;
                if (!String.IsNullOrEmpty(EndDayDelayFlag) && !String.IsNullOrEmpty(thisDateRecord.EndDayDelayExplanation))
                    ViewData["ExplText"] = thisDateRecord.EndDayDelayExplanation;
            }

            return View();
        }



        public async Task<IActionResult> IntervalTypeSet(int? id, string dateRangeSearch, int? page)
        {
            ViewData["dateRangeSearch"] = dateRangeSearch;
            ViewData["page"] = page;
            string dateSortParam = null;

            if (id == null)
            {
                TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";
                return RedirectToAction("Index", new { dateSortParam, dateRangeSearch, page });
            }

            var thisInterval = await _context.IntervalRecords.FindAsync(id);

            if (thisInterval == null)
            {
                TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";
                return RedirectToAction("Index", new { dateSortParam, dateRangeSearch, page });
            }

            //return values in edit mode
            //choosed radio button
            switch (thisInterval.IntervalType)
            {
                case "Almoço":
                    ViewData["AlmocoChecked"] = "checked";
                    break;
                case "Café":
                    ViewData["CafeChecked"] = "checked";
                    break;
                case "Hospital":
                    ViewData["HospitalChecked"] = "checked";
                    break;
                case "Outro":
                    ViewData["OutroChecked"] = "checked";
                    break;
            }

            if (String.Compare(thisInterval.IntervalType, "Outro") == 0)
                ViewData["intervalType"] = thisInterval.IntervalType;

            ViewData["InervalDate"] = _context.DayRecords.FirstOrDefault(c => c.Id == thisInterval.DayRecordId).Data.ToShortDateString();

            return View(thisInterval);
        }


        [HttpPost]
        [ActionName("IntervalTypeSet")]
        public async Task<IActionResult> IntervalTypeSetPost(int? id, string intervalType, string intervalTypeCustom, string dateRangeSearch, int? page)
        {
            ViewData["dateRangeSearch"] = dateRangeSearch;
            ViewData["page"] = page;
            string dateSortParam = null;

            if (id == null)
            {
                TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";
                return RedirectToAction("Index", new { dateSortParam, dateRangeSearch, page });
            }

            var thisInterval = await _context.IntervalRecords.FindAsync(id);

            if (thisInterval == null)
            {
                TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";
                return RedirectToAction("Index", new { dateSortParam, dateRangeSearch, page });
            }

            if (String.IsNullOrWhiteSpace(intervalType))
            {
                ModelState.AddModelError(String.Empty, "Por favor, adiciona o tipo de Intervalo.");
            }
            else if (String.Compare(intervalType, "Outro") == 0 && String.IsNullOrWhiteSpace(intervalTypeCustom))
            {
                ModelState.AddModelError(String.Empty, "Se escolheu outro tipo de Intervalo, por favor adiciona a descrição.");
            }
            else if (String.Compare(intervalType, "Outro") == 0 && String.IsNullOrWhiteSpace(intervalTypeCustom) && intervalTypeCustom.Length > 50)
            {
                ModelState.AddModelError(String.Empty, "O tipo de Intervalo tem de conter máximo 50 caracteres.");
            }
            else
            {
                try
                {
                    if (String.Compare(intervalType, "Outro") == 0)
                        thisInterval.IntervalType = intervalTypeCustom;
                    else
                        thisInterval.IntervalType = intervalType;
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "O tipo de Intervalo foi guardado com sucesso.";

                    return RedirectToAction("Index", new { dateSortParam, dateRangeSearch, page });
                }
                catch (Exception)
                {
                    TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";
                }
            }

            //return values in edit mode
            //choosed radio button
            switch (thisInterval.IntervalType)
            {
                case "Almoço":
                    ViewData["AlmocoChecked"] = "checked";
                    break;
                case "Café":
                    ViewData["CafeChecked"] = "checked";
                    break;
                case "Hospital":
                    ViewData["HospitalChecked"] = "checked";
                    break;
                case "Outro":
                    ViewData["OutroChecked"] = "checked";
                    break;
            }

            if (String.Compare(thisInterval.IntervalType, "Outro") == 0)
                ViewData["intervalType"] = thisInterval.IntervalType;

            ViewData["InervalDate"] = _context.DayRecords.FirstOrDefault(c => c.Id == thisInterval.DayRecordId).Data.ToShortDateString();

            return View(thisInterval);
        }


    }
}
