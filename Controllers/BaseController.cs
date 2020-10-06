using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attendance_Performance_Control.Data;
using Attendance_Performance_Control.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Attendance_Performance_Control.Controllers
{
    public class BaseController : Controller
    {

        protected readonly ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public string GetDefaultRangeDataPicker()
        {
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            return String.Concat(firstDayOfMonth.ToShortDateString(), " - ", lastDayOfMonth.ToShortDateString());
        }

        public async Task<List<IntervalRecord>> GetIntervalsList(int dayRecordId)
        {
            //Get All Intervals in db with specific RecordId
            var listOfIntervals = await _context.IntervalRecords.Where(c => c.DayRecordId == dayRecordId).ToListAsync();

            return listOfIntervals;
        }

        public TimeSpan GetTotalIntervalHoursPorDay(int recordId)
        {
            var intervalsListPorRecordId = GetIntervalsList(recordId).Result;

            TimeSpan intervalSum = new TimeSpan();

            foreach (var interval in intervalsListPorRecordId)
            {
                intervalSum += interval.EndTime - interval.StartTime;
            }

            return intervalSum;
        }


        public async Task<DateTime> GetDateStartTime(int dayRecordId)
        {
            //Get All TimeRecords in db with specific RecordId
            var listOfTimeRecords = await _context.TimeRecords.Where(c => c.DayRecordId == dayRecordId).OrderBy(c => c.StartTime).ToListAsync();
            var dateStartTime = listOfTimeRecords.First().StartTime;

            return dateStartTime;
        }

        public async Task<DateTime?> GetDateEndTime(int dayRecordId)
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

        public TimeSpan? GetTotalWorkHoursPorDay(int recordId)
        {
            var intervalsSum = GetTotalIntervalHoursPorDay(recordId);

            var dayStartTime = GetDateStartTime(recordId).Result;
            var dayEndTime = GetDateEndTime(recordId).Result;

            return dayEndTime.HasValue ? dayEndTime - dayStartTime - intervalsSum : null;
        }

        public bool UserPertenceToDepartment (string userId, int departId)
        {
            bool pertenceToDept = false;

            var user = _userManager.FindByIdAsync(userId).Result;

            //get all occupations of this Department
            var occupationListByDept = _context.Occupations.Where(c => c.DepartmentId == departId).ToList();

            //ger all Records of users that pertence to this list of occupations that pertence to this Department
            foreach (var occupation in occupationListByDept)
            {
                    if (user.OccupationId == occupation.Id)
                    {
                    pertenceToDept = true;
                    break;
                    }
            }

            return pertenceToDept;
        }

        public List<ApplicationUser> ListOfAllUsersByDepartment(int departId)
        {
            List<ApplicationUser> usersList = new List<ApplicationUser>();
            //get all occupations of this Department
            var occupationListByDept = _context.Occupations.Where(c => c.DepartmentId == departId).ToList();

            //ger all Records of users that pertence to this list of occupations that pertence to this Department
            foreach (var occupation in occupationListByDept)
            {
                foreach (var user in _context.Users)
                {
                    if (user.OccupationId == occupation.Id)
                    {
                        usersList.Add(user);
                    }
                }
            }

            return usersList;
        }
    }
}
