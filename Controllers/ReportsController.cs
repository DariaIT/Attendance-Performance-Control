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
        public async Task<IActionResult> Index(string dateRangeSearch, string searchByUser)
        {
            //set datarange picker to null value
            //by default insteed of null return: 21/09/2020-21/09/2020 with DateTime.Now
            if (String.Compare(dateRangeSearch, String.Concat(DateTime.Now.ToShortDateString()," ", "-", " ", DateTime.Now.ToShortDateString())) == 0)
                dateRangeSearch = "";

            List<ReportsViewModel> listOfRecords = new List<ReportsViewModel>();

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

            //if Admin and search by user is empty => return all records of all users
            if (_userManager.IsInRoleAsync(user, "Admin").Result && String.IsNullOrEmpty(searchByUser))
                user = null;
            //if Admin and search user has value => return search user records
            else if (_userManager.IsInRoleAsync(user, "Admin").Result && !String.IsNullOrEmpty(searchByUser))
                user = await _userManager.FindByIdAsync(searchByUser);
            //else, if User => return records of current user

            //return list of records of given user
            //return list of records in dateRangeSearch period: default "This Month" or defined by client
            //if user=null, return all records of all users (Admin View)
            listOfRecords = await CreateReportsViewModel(startDate, endDate, user);

            ViewData["Users"] = new SelectList(_context.Users, "Id", "FullName", searchByUser);
            ViewData["searchByUser"] = searchByUser;

            return View(listOfRecords);
        }


        public async Task<List<ReportsViewModel>> CreateReportsViewModel(DateTime startDate, DateTime endDate, ApplicationUser user)
        {

            //Initialize
            var listOfReportsViewModel = new List<ReportsViewModel>();
            var recordsListOfCurrentUser = new List<DayRecord>();

            //Get All Records of given user in period "startDate to endDate"
            //if user=null, get all records (Admin View)
            if (user==null)
               recordsListOfCurrentUser = await _context.DayRecords.Where(c => c.Data.Date >= startDate && c.Data.Date <= endDate).ToListAsync();
            else
             recordsListOfCurrentUser =
                await _context.DayRecords.Where(c => c.UserId == user.Id && c.Data.Date >= startDate && c.Data.Date <= endDate).ToListAsync();

            //create list of ReportsViewModel
            foreach (var record in recordsListOfCurrentUser)
            {
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
                    TotalHoursForWork =((TimeSpan) GetTotalWorkHoursPorDay(record.Id)).ToString("hh\\:mm\\:ss"),
                    TotalHoursForIntervals = GetTotalIntervalHoursPorDay(record.Id).ToString("hh\\:mm\\:ss")
                };

                //add all records except of today
                if (reportRecord.Data < DateTime.Now)
                {
                    listOfReportsViewModel.Add(reportRecord);
                }

            }

            //return list of UserRecordViewModel
            return listOfReportsViewModel;
        }
    }
}
