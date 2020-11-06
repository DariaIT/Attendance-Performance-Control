using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attendance_Performance_Control.Data;
using Attendance_Performance_Control.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Attendance_Performance_Control.Controllers
{
    public class BaseController : Controller
    {

        protected readonly ApplicationDbContext _context;
        protected readonly ReadOnlyBigalconDbContext _contextBigalcon;
        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseController(
            ApplicationDbContext context,
            //ReadOnlyBigalconDbContext contextBigalcon,
        UserManager<ApplicationUser> userManager)
        {
            _context = context;
            //_contextBigalcon = contextBigalcon;
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

        public bool CheckIfOneRecordNotClosed(int dayRecordId)
        {
            bool IsOneRecordNotClosed = false;

            //Get All TimeRecords in db with specific DayRecordId
            var listOfTimeRecords = _context.TimeRecords.Where(c => c.DayRecordId == dayRecordId).OrderBy(c => c.StartTime).ToList();

            return IsOneRecordNotClosed = listOfTimeRecords.Last().EndTime == null ? IsOneRecordNotClosed = true : IsOneRecordNotClosed;
        }

        public void SetNormalizeTimetableInTimerLost(DayRecord record)
        {
            var user = _userManager.FindByIdAsync(record.UserId).Result;
            var startWorkTime = (DateTime)user.StartWorkTime;
            var endWorkTime = (DateTime)user.EndWorkTime;
            var startLunchTime = user.StartLunchTime;
            var endLunchTime = user.EndLunchTime;

            //change db.DayRecord.Data value - i.e DateTime.Add(TimeSpan time)
            record.Data = record.Data.Date.Add(startWorkTime.TimeOfDay);
            record.StartDayDelayExplanation = null;
            record.EndDayDelayExplanation = null;
            //find all TimeRecords (with this record.Id) and remove it 
            foreach (var timerecord in _context.TimeRecords)
            {
                if (timerecord.DayRecordId == record.Id)
                    _context.TimeRecords.Remove(timerecord);
            }
            //Add two normalized entries to db.TimeRecords 9:00 - 13:00 and 14:00 - 18:00
            var timeSpan1 = new TimeSpan(13, 0, 0);
            var startlunch = startLunchTime == null ? timeSpan1 : ((DateTime)startLunchTime).TimeOfDay;
            var timeSpan2 = new TimeSpan(14, 0, 0);
            var endLunch = endLunchTime == null ? timeSpan2 : ((DateTime)endLunchTime).TimeOfDay;

            var newTimeRecordBeforeLunch = new TimeRecord()
            {
                StartTime = record.Data.Date.Add(startWorkTime.TimeOfDay),
                EndTime = record.Data.Date.Add(startlunch),
                DayRecordId = record.Id
            };
            var newTimeRecordAfterLunch = new TimeRecord()
            {
                StartTime = record.Data.Date.Add(endLunch),
                EndTime = record.Data.Date.Add(endWorkTime.TimeOfDay),
                DayRecordId = record.Id
            };
            _context.TimeRecords.Add(newTimeRecordBeforeLunch);
            _context.TimeRecords.Add(newTimeRecordAfterLunch);

            //remove Intervals with this Record.Id
            foreach (var intervalrecord in _context.IntervalRecords)
            {
                if (intervalrecord.DayRecordId == record.Id)
                    _context.IntervalRecords.Remove(intervalrecord);
            }

            //add new normalize Inerval Record - 13:00 -14:00 Lunch

            var normalizeInterval = new IntervalRecord()
            {
                StartTime = record.Data.Date.Add(startlunch),
                EndTime = record.Data.Date.Add(endLunch),
                DayRecordId = record.Id,
                IntervalType = "Almoço"
            };
            _context.IntervalRecords.Add(normalizeInterval);
            _context.SaveChanges();
        }


        public TimeSpan? GetTotalWorkHoursPorDay(int recordId)
        {
            var intervalsSum = GetTotalIntervalHoursPorDay(recordId);

            var dayStartTime = GetDateStartTime(recordId).Result;
            var dayEndTime = GetDateEndTime(recordId).Result;

            return dayEndTime.HasValue ? dayEndTime - dayStartTime - intervalsSum : null;
        }

        public bool UserPertenceToDepartment(string userId, int departId)
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

            var adminId = GetAdminUserId();
            //ger all Records of users that pertence to this list of occupations that pertence to this Department
            foreach (var occupation in occupationListByDept)
            {
                foreach (var user in _context.Users)
                {
                    if (user.OccupationId == occupation.Id && user.Id != adminId)
                    {
                        usersList.Add(user);
                    }
                }
            }

            return usersList;
        }

        public string GetAdminUserId()
        {
            var adminId = "";
            foreach (var user in _context.Users)
            {
                if (_userManager.IsInRoleAsync(user, "Admin").Result)
                    adminId = user.Id;
            }
            return adminId;
        }


        public class JsonObj
        {
            public string x { get; set; }
            public double y { get; set; }
        }

        //data: [{
        //x: '01/02/2019', <- Date
        //y: 1 <- Total Hours
        //},
        //... ]
        //Could be zero or multiple users
        public string GetJsonDataForGrafic(List<ReportsViewModel> listOfRecords)
        {
            var jsonList = new List<JsonObj>();
            bool IsAdded = false;

            //this part for test grafic seeding
            //for(var i=60; i>=0; i--)
            //{
            //    var data = DateTime.Now.AddDays(-i).ToShortDateString();
            //    Random number = new Random();
            //    var item = new JsonObj() { x= data, y= (number.Next(1,50)+0.2) };
            //    jsonList.Add(item);
            //}

            foreach (var record in listOfRecords)
            {
                var thisDate = record.Data.ToShortDateString();
                //convert to int hours for Chart.js
                var convertedTime = DateTime.Parse(record.TotalHoursForWork);
                //transform everything in seconds and transform in hours 2.3h
                double hours = ((convertedTime.Hour * 60 + convertedTime.Minute) * 60 + convertedTime.Second) / 3600.0;

                if (jsonList.Count != 0)
                {
                    //check if record with this date allready exist in jsonList
                    foreach (var item in jsonList)
                    {
                        //if date allready exist in jsonList - add houras to y
                        if (String.Compare(item.x, thisDate) == 0)
                        {
                            item.y += hours;
                            IsAdded = true;
                            break;
                        }
                    }
                    //date do not exist yet - add to jsonList
                    if (!IsAdded)
                    {
                        var thisJsonObj = new JsonObj() { x = thisDate, y = hours };
                        jsonList.Add(thisJsonObj);
                    }
                }
                else
                {
                    var thisJsonObj = new JsonObj() { x = thisDate, y = hours };
                    jsonList.Add(thisJsonObj);
                }
            }

            //serialize list to Json string and pass to Controller and by ViewData pass to Chart.js
            var serializedList = JsonSerializer.Serialize(jsonList);

            return serializedList;
        }


        public string TotalWorkHoursFromRecordsList(List<ReportsViewModel> listOfRecords)
        {
            int hours = 0;
            int minuts = 0;
            int seconds = 0;
            foreach (var record in listOfRecords)
            {
                hours += DateTime.Parse(record.TotalHoursForWork).Hour;
                minuts += DateTime.Parse(record.TotalHoursForWork).Minute;
                seconds += DateTime.Parse(record.TotalHoursForWork).Second;
            }
            //format hours, minuts and seconds
            //seconds to minuts
            int minInt = seconds / 60;
            minuts += minInt;
            seconds -= minInt * 60;
            //minuts to hours
            int hourInt = minuts / 60;
            hours += hourInt;
            minuts -= hourInt * 60;

            //to string time format ex 9 -> 09
            string hoursStr = hours > 9 ? hours.ToString() : String.Concat("0", hours);
            string minutsStr = minuts > 9 ? minuts.ToString() : String.Concat("0", minuts);
            string secondsStr = seconds > 9 ? seconds.ToString() : String.Concat("0", seconds);

            return String.Concat(hoursStr, ":", minutsStr, ":", secondsStr);
        }

        public string TotalIntervalsHoursFromRecordsList(List<ReportsViewModel> listOfRecords)
        {
            int hours = 0;
            int minuts = 0;
            int seconds = 0;
            foreach (var record in listOfRecords)
            {
                hours += DateTime.Parse(record.TotalHoursForIntervals).Hour;
                minuts += DateTime.Parse(record.TotalHoursForIntervals).Minute;
                seconds += DateTime.Parse(record.TotalHoursForIntervals).Second;
            }
            //format hours, minuts and seconds
            //seconds to minuts
            int minInt = seconds / 60;
            minuts += minInt;
            seconds -= minInt * 60;
            //minuts to hours
            int hourInt = minuts / 60;
            hours += hourInt;
            minuts -= hourInt * 60;

            //to string time format ex 9 -> 09
            string hoursStr = hours > 9 ? hours.ToString() : String.Concat("0", hours);
            string minutsStr = minuts > 9 ? minuts.ToString() : String.Concat("0", minuts);
            string secondsStr = seconds > 9 ? seconds.ToString() : String.Concat("0", seconds);

            return String.Concat(hoursStr, ":", minutsStr, ":", secondsStr);
        }
    }
}
