using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attendance_Performance_Control.Data;
using Attendance_Performance_Control.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Attendance_Performance_Control.Controllers
{
    public class ReportsController : BaseController
    {

        public ReportsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(string dateRangeSearch, string searchByUser, int? searchByDept)
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
            //if Admin and search user has value => return search user records
            else if (IsAdmin && !String.IsNullOrEmpty(searchByUser))
                user = await _userManager.FindByIdAsync(searchByUser);
            //else, if User => return records of current user

            //return list of records of given user
            //return list of records in dateRangeSearch period: default "This Month" or defined by client
            //if user=null, return all records of all users (Admin View)
            var listOfRecords = await CreateReportsViewModel(startDate, endDate, user, searchByDept);

            //if searchByDept is not null, set user search to null value, because it is concurent searching
            if (searchByDept != null && searchByUser != null)
            {
                if (UserPertenceToDepartment(searchByUser, (int)searchByDept))
                {
                    ViewData["Users"] = new SelectList(ListOfAllUsersByDepartment((int)searchByDept), "Id", "FullName", searchByUser);
                    ViewData["searchByUser"] = searchByUser;
                }
                //user do not pertence to department
                else
                {
                    ViewData["Users"] = new SelectList(ListOfAllUsersByDepartment((int)searchByDept), "Id", "FullName");
                    ViewData["searchByUser"] = null;
                }
            }
            else if (searchByDept != null && searchByUser == null)
            {
                ViewData["Users"] = new SelectList(ListOfAllUsersByDepartment((int)searchByDept), "Id", "FullName");
                ViewData["searchByUser"] = null;
            }
            else
            {
                var adminId = GetAdminUserId();
                ViewData["Users"] = new SelectList(_context.Users.Where(c => c.Id != adminId), "Id", "FullName", searchByUser);
                ViewData["searchByUser"] = searchByUser;
            }

            ViewData["Departments"] = new SelectList(_context.Departments, "Id", "DepartmentName", searchByDept);
            ViewData["searchByDept"] = searchByDept;

            var daysRange = endDate - startDate;
            var days = daysRange.Days;

            ViewData["GraficDays"] = days;
            ViewData["JsonDataForGrafic"] = GetJsonDataForGrafic(listOfRecords);

            //totals for graf and reports
            var totalWork = TotalWorkHoursFromRecordsList(listOfRecords);
            var totalInterv = TotalIntervalsHoursFromRecordsList(listOfRecords);
            ViewData["TotalWork"] = totalWork;
            ViewData["TotalIntervals"] = totalInterv;

            //data for report title
            ViewData["ReportTitle"] = "Relatorio - Prévia Safe - Saúde Ocupacional, Higiene e Segurança S. A.";
            ViewData["ReportData"] = DateTime.Now.ToShortDateString();

            if (searchByDept != null)
                ViewData["ReportDepart"] = _context.Departments.Where(c => c.Id == searchByDept).FirstOrDefault().DepartmentName;
            if (user != null)
            {
                var userOccupation = _context.Occupations.FirstOrDefault(c => c.Id == user.OccupationId);
                var userDepartment = _context.Departments.FirstOrDefault(c => c.Id == userOccupation.DepartmentId);

                ViewData["ReportName"] = user.FullName;
                if (searchByDept == null)
                    ViewData["ReportDepart"] = userDepartment.DepartmentName;
                ViewData["ReportOccup"] = userOccupation.OccupationName;
            }

            return View(listOfRecords);
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost(string dateRangeSearch, string searchByUser, int? searchByDept)
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
            //if Admin and search user has value => return search user records
            else if (IsAdmin && !String.IsNullOrEmpty(searchByUser))
                user = await _userManager.FindByIdAsync(searchByUser);
            //else, if User => return records of current user

            //return list of records of given user
            //return list of records in dateRangeSearch period: default "This Month" or defined by client
            //if user=null, return all records of all users (Admin View)
            var listOfRecords = await CreateReportsViewModel(startDate, endDate, user, searchByDept);

            //if searchByDept is not null, set user search to null value, because it is concurent searching
            if (searchByDept != null && searchByUser != null)
            {
                if (UserPertenceToDepartment(searchByUser, (int)searchByDept))
                {
                    ViewData["Users"] = new SelectList(ListOfAllUsersByDepartment((int)searchByDept), "Id", "FullName", searchByUser);
                    ViewData["searchByUser"] = searchByUser;
                }
                //user do not pertence to department
                else
                {
                    ViewData["Users"] = new SelectList(ListOfAllUsersByDepartment((int)searchByDept), "Id", "FullName");
                    ViewData["searchByUser"] = null;
                }
            }
            else if (searchByDept != null && searchByUser == null)
            {
                ViewData["Users"] = new SelectList(ListOfAllUsersByDepartment((int)searchByDept), "Id", "FullName");
                ViewData["searchByUser"] = null;
            }
            else
            {
                var adminId = GetAdminUserId();
                ViewData["Users"] = new SelectList(_context.Users.Where(c=>c.Id!=adminId), "Id", "FullName", searchByUser);
                ViewData["searchByUser"] = searchByUser;
            }

            ViewData["Departments"] = new SelectList(_context.Departments, "Id", "DepartmentName", searchByDept);
            ViewData["searchByDept"] = searchByDept;

            var daysRange = endDate - startDate;
            var days = daysRange.Days;

            ViewData["GraficDays"] = days;
            ViewData["JsonDataForGrafic"] = GetJsonDataForGrafic(listOfRecords);

            //totals for graf and reports
            var totalWork = TotalWorkHoursFromRecordsList(listOfRecords);
            var totalInterv = TotalIntervalsHoursFromRecordsList(listOfRecords);
            ViewData["TotalWork"] = totalWork;
            ViewData["TotalIntervals"] = totalInterv;

            //data for report title
            ViewData["ReportTitle"] = "Relatorio - Prévia Safe - Saúde Ocupacional, Higiene e Segurança S. A.";
            ViewData["ReportData"] = DateTime.Now.ToShortDateString();

            if (searchByDept != null)
                ViewData["ReportDepart"] = _context.Departments.Where(c => c.Id == searchByDept).FirstOrDefault().DepartmentName;
            if (user != null)
            {
                var userOccupation = _context.Occupations.FirstOrDefault(c => c.Id == user.OccupationId);
                var userDepartment = _context.Departments.FirstOrDefault(c => c.Id == userOccupation.DepartmentId);

                ViewData["ReportName"] = user.FullName;
                if (searchByDept == null)
                    ViewData["ReportDepart"] = userDepartment.DepartmentName;
                ViewData["ReportOccup"] = userOccupation.OccupationName;
            }

        return View(listOfRecords);
        }


        public async Task<List<ReportsViewModel>> CreateReportsViewModel(DateTime startDate, DateTime endDate, ApplicationUser user, int? searchByDept)
        {

            //Initialize
            var listOfReportsViewModel = new List<ReportsViewModel>();
            var recordsListOfCurrentUser = new List<DayRecord>();

            //Get All Records of given user in period "startDate to endDate"
            //if user=null, get all records (Admin View)
            //if searchByDept != null, get all users of this department
            if (searchByDept != null)
            {
                //if searchbyUser not null and user pertence to department
                if (user != null && UserPertenceToDepartment(user.Id, (int)searchByDept))
                {
                    recordsListOfCurrentUser =
                   await _context.DayRecords.Where(c => c.UserId == user.Id && c.Data.Date >= startDate && c.Data.Date <= endDate).ToListAsync();
                }
                else
                {
                    //user is null or do not pertence to Department
                    //ger all Records of users that pertence to this Department
                    foreach (var us in _context.Users)
                    {
                        if (UserPertenceToDepartment(us.Id, (int)searchByDept))
                        {
                            var tempRecordsList = _context.DayRecords.Where(c => c.User.Id == us.Id && c.Data.Date >= startDate && c.Data.Date <= endDate).ToList();
                            recordsListOfCurrentUser.AddRange(tempRecordsList);
                        }
                    }
                }
            }
            //return all Records by Date
            else if (user == null && searchByDept == null)
                recordsListOfCurrentUser = await _context.DayRecords.Where(c => c.Data.Date >= startDate && c.Data.Date <= endDate).ToListAsync();
            //return all records of current user (not Admin View)
            else
                recordsListOfCurrentUser =
                   await _context.DayRecords.Where(c => c.UserId == user.Id && c.Data.Date >= startDate && c.Data.Date <= endDate).ToListAsync();

            //create list of ReportsViewModel
            foreach (var record in recordsListOfCurrentUser)
            {
                //add all records except of today
                if (record.Data.Date < DateTime.Now.Date)
                {
                    //check if exist TimeRecord with EndTime = null
                    var IsOneRecordNotClosed = CheckIfOneRecordNotClosed(record.Id);
                    if (IsOneRecordNotClosed)
                        SetNormalizeTimetableInTimerLost(record);

                    var thisUser = await _userManager.FindByIdAsync(record.UserId);
                    var userOccupation = await _context.Occupations.FirstOrDefaultAsync(c => c.Id == thisUser.OccupationId);
                    var userDepartment = await _context.Departments.FirstOrDefaultAsync(c => c.Id == userOccupation.DepartmentId);
                    //create and initialize UserRecordViewModel
                    var reportRecord = new ReportsViewModel()
                    {
                        DayRecordsId = record.Id,
                        Data = record.Data,
                        UserDepartment = userDepartment.DepartmentName,
                        UserOccupation = userOccupation.OccupationName,
                        DayStartTime = await GetDateStartTime(record.Id),
                        StartDayDelayExplanation = record.StartDayDelayExplanation,
                        DayEndTime = (DateTime)await GetDateEndTime(record.Id),
                        EndDayDelayExplanation = record.EndDayDelayExplanation,
                        TotalHoursForWork = ((TimeSpan)GetTotalWorkHoursPorDay(record.Id)).ToString("hh\\:mm\\:ss"),
                        TotalHoursForIntervals = GetTotalIntervalHoursPorDay(record.Id).ToString("hh\\:mm\\:ss")
                    };

                    listOfReportsViewModel.Add(reportRecord);
                }
            }

            //return list of UserRecordViewModel
            return listOfReportsViewModel;
        }

        public async Task<IActionResult> DetailsRecord (int? id, string dateRangeSearch, string searchByUser, int? searchByDept)
        {
            ViewData["dateRangeSearch"] = dateRangeSearch;
            ViewData["searchByUser"] = searchByUser;
            ViewData["searchByDept"] = searchByDept;

            if (id == null)
            {
                TempData["Failure"] = "Algo correu errado, por favor, tente novamente ou contacte Administrador do Sistema.";
                return RedirectToAction("Index", new { dateRangeSearch, searchByUser, searchByDept });
            }

            var record = await _context.DayRecords.FindAsync(id);
            var user = await _userManager.FindByIdAsync(record.UserId);

            ViewData["Nome"] = user.FullName;
            ViewData["Email"] = user.Email;

            var intervalsList = await _context.IntervalRecords.Where(c => c.DayRecordId == id).ToListAsync();

            ViewData["IntervalsList"] = intervalsList;

            var userOccupation = await _context.Occupations.FirstOrDefaultAsync(c => c.Id == user.OccupationId);
            var userDepartment = await _context.Departments.FirstOrDefaultAsync(c => c.Id == userOccupation.DepartmentId);
            //create and initialize UserRecordViewModel
            var reportRecord = new ReportsViewModel()
            {
                DayRecordsId = record.Id,
                Data = record.Data,
                UserDepartment = userDepartment.DepartmentName,
                UserOccupation = userOccupation.OccupationName,
                DayStartTime = await GetDateStartTime(record.Id),
                StartDayDelayExplanation = record.StartDayDelayExplanation,
                DayEndTime = (DateTime)await GetDateEndTime(record.Id),
                EndDayDelayExplanation = record.EndDayDelayExplanation,
                TotalHoursForWork = ((TimeSpan)GetTotalWorkHoursPorDay(record.Id)).ToString("hh\\:mm\\:ss"),
                TotalHoursForIntervals = GetTotalIntervalHoursPorDay(record.Id).ToString("hh\\:mm\\:ss")
            };

            return View(reportRecord);
        }
    }

}
