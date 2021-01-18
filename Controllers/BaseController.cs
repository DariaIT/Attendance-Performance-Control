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

            //remove Intervals with this Record.Id
            foreach (var intervalrecord in _context.IntervalRecords)
            {
                if (intervalrecord.DayRecordId == record.Id)
                    _context.IntervalRecords.Remove(intervalrecord);
            }
            //Add two normalized entries to db.TimeRecords 9:00 - 13:00 and 14:00 - 18:00
            //var timeSpan1 = new TimeSpan(13, 0, 0);
            //var startlunch = startLunchTime == null ? timeSpan1 : ((DateTime)startLunchTime).TimeOfDay;
            //var timeSpan2 = new TimeSpan(14, 0, 0);
            //var endLunch = endLunchTime == null ? timeSpan2 : ((DateTime)endLunchTime).TimeOfDay;

            //if startLunchTime and endLunchTime are not empty -> add two TimeRecords + Interval
            if (startLunchTime.HasValue && endLunchTime.HasValue)
            {
                var newTimeRecordBeforeLunch = new TimeRecord()
                {
                    StartTime = record.Data.Date.Add(startWorkTime.TimeOfDay),
                    EndTime = record.Data.Date.Add(((DateTime)startLunchTime).TimeOfDay),
                    DayRecordId = record.Id
                };
                var newTimeRecordAfterLunch = new TimeRecord()
                {
                    StartTime = record.Data.Date.Add(((DateTime)endLunchTime).TimeOfDay),
                    EndTime = record.Data.Date.Add(endWorkTime.TimeOfDay),
                    DayRecordId = record.Id
                };
                _context.TimeRecords.Add(newTimeRecordBeforeLunch);
                _context.TimeRecords.Add(newTimeRecordAfterLunch);

                //add new normalize Inerval Record - 13:00 -14:00 Lunch

                var normalizeInterval = new IntervalRecord()
                {
                    StartTime = record.Data.Date.Add(((DateTime)startLunchTime).TimeOfDay),
                    EndTime = record.Data.Date.Add(((DateTime)endLunchTime).TimeOfDay),
                    DayRecordId = record.Id,
                    IntervalType = "Almoço"
                };
                _context.IntervalRecords.Add(normalizeInterval);
                _context.SaveChanges();
            }
            //if startLunchTime and endLunchTime are empty->add TimeRecord without interval
            else
            {
                var newTimeRecordWithoutLunch = new TimeRecord()
                {
                    StartTime = record.Data.Date.Add(startWorkTime.TimeOfDay),
                    EndTime = record.Data.Date.Add(endWorkTime.TimeOfDay),
                    DayRecordId = record.Id
                };
                _context.TimeRecords.Add(newTimeRecordWithoutLunch);
                _context.SaveChanges();
            }
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

        //function calculate number of working days in period of time
        //i.e. days  that not Saterday, not Sunday, no Bank Holidays
        public int GetNumberOfWorkingDaysInPeriodOfTime(DateTime startDate, DateTime endDate)
        {
            int workingDays = 0;
            var bankHolidaysList = _context.BankHolidays.Where(c => c.Data.Date >= startDate.Date && c.Data.Date <= endDate.Date).ToList();

            while (startDate.Date <= endDate.Date)
            {
                var isHoliday = bankHolidaysList.FirstOrDefault(c => c.Data.Date == startDate.Date);
                if (startDate.DayOfWeek != DayOfWeek.Saturday && startDate.DayOfWeek != DayOfWeek.Sunday && isHoliday == null)
                {
                    workingDays++;
                }

                startDate = startDate.AddDays(1);
            }
            return workingDays;
        }

        //calculate number of expected working days
        //Oficial Working Days - User Holidays(if it is not Suterday, Sunday, Bank Holyday)
        public int GetNumberOfExpectedWorkingDaysInPeriodOfTime(DateTime startDate, DateTime endDate, ApplicationUser user,
            int workingDays, int holidaysDays)
        {
            int expectedWorkingDays = workingDays;

            //if any day of holiday overlap with Bank Holyday ou Saturday or Sunday -> do not subtract from working days
            if (holidaysDays != 0)
            {
                var holidaysList = _context.UserHolidays.Where(c => c.UserId == user.Id && c.HolidayDay.Date >= startDate.Date &&
                c.HolidayDay.Date <= endDate.Date).ToList();

                var bankHolidayList = _context.BankHolidays.Where(c => c.Data.Date >= startDate.Date &&
                c.Data.Date <= endDate.Date).ToList();

                foreach (var holiday in holidaysList)
                {
                    //not Saturday, not Sunday, not Bank Holiday
                    var isBankHoliday = bankHolidayList.FirstOrDefault(c => c.Data.Date == holiday.HolidayDay.Date);
                    if (holiday.HolidayDay.DayOfWeek != DayOfWeek.Saturday || holiday.HolidayDay.DayOfWeek != DayOfWeek.Sunday
                        || isBankHoliday == null)
                    {
                        expectedWorkingDays--;
                    }
                }
            }
            return expectedWorkingDays;
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

        //This function return:
        // If user!= null -> return Json String with Complete days for this user
        // If user==null -> return Json String with some of all working hours of all users
        public string GetJsonDataOfCompleteWorkDaysForGrafic(List<ReportsViewModel> listOfRecords, ApplicationUser user)
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
                //renew variable before each circle
                IsAdded = false;

                //if we do send json information for concrete user
                if (user != null)
                {
                    var oficialWorkHours = GetOficialWorkHours(user);
                    double oficialConvertedHours = ((oficialWorkHours.Hours * 60 + oficialWorkHours.Minutes) * 60 + oficialWorkHours.Seconds) / 3600.0;
                    //if registered hours is more then oficial work hours - add to json list
                    if (hours > oficialConvertedHours)
                    {
                        var thisJsonObj = new JsonObj() { x = thisDate, y = hours };
                        jsonList.Add(thisJsonObj);
                    }
                }
                //if we send json information (some) for all users
                else
                {
                    if (jsonList.Count != 0)
                    {
                        //check if record with this date allready exist in jsonList
                        foreach (var item in jsonList)
                        {
                            //if date allready exist in jsonList - add hours to y
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
                    //first element
                    else
                    {
                        var thisJsonObj = new JsonObj() { x = thisDate, y = hours };
                        jsonList.Add(thisJsonObj);
                    }
                }
            }

            //serialize list to Json string and pass to Controller and by ViewData pass to Chart.js
            var serializedList = JsonSerializer.Serialize(jsonList);

            return serializedList;
        }


        //data: [{
        //x: '01/02/2019', <- Date
        //y: 1 <- Total Hours
        //},
        //... ]
        //Could be zero or multiple users

        //This function return Json String with Incomplete Days for concrete user
        public string GetJsonDataOfIncompleteWorkDaysForGrafic(List<ReportsViewModel> listOfRecords, ApplicationUser user)
        {
            var jsonList = new List<JsonObj>();

            //this part for test grafic seeding
            //for(var i=60; i>=0; i--)
            //{
            //    var data = DateTime.Now.AddDays(-i).ToShortDateString();
            //    Random number = new Random();
            //    var item = new JsonObj() { x= data, y= (number.Next(1,50)+0.2) };
            //    jsonList.Add(item);
            //}

            //if we do send json information for concrete user
            if (user != null)
            {
                foreach (var record in listOfRecords)
                {
                    var thisDate = record.Data.ToShortDateString();
                    //convert to int hours for Chart.js
                    var convertedTime = DateTime.Parse(record.TotalHoursForWork);
                    //transform everything in seconds and transform in hours 2.3h
                    double hours = ((convertedTime.Hour * 60 + convertedTime.Minute) * 60 + convertedTime.Second) / 3600.0;

                    var oficialWorkHours = GetOficialWorkHours(user);
                    double oficialConvertedHours = ((oficialWorkHours.Hours * 60 + oficialWorkHours.Minutes) * 60 + oficialWorkHours.Seconds) / 3600.0;
                    //if registered hours is less then oficial work hours - add to json list
                    if (hours < oficialConvertedHours)
                    {
                        var thisJsonObj = new JsonObj() { x = thisDate, y = hours };
                        jsonList.Add(thisJsonObj);
                    }

                }
            }
            //serialize list to Json string and pass to Controller and by ViewData pass to Chart.js
            var serializedList = JsonSerializer.Serialize(jsonList);

            return serializedList;
        }

        //return Timespan of oficial working hours of user, defined in system
        public TimeSpan GetOficialWorkHours(ApplicationUser user)
        {
            var oficialWorkHours = ((DateTime)user.EndWorkTime).TimeOfDay - ((DateTime)user.StartWorkTime).TimeOfDay;
            //if has lunch hours
            if (user.StartLunchTime != null && user.EndLunchTime != null)
            {
                var intervalHours = ((DateTime)user.EndLunchTime).TimeOfDay - ((DateTime)user.StartLunchTime).TimeOfDay;
                oficialWorkHours -= intervalHours;
            }
            return oficialWorkHours;
        }

        //return Timespan of oficial interval hours of user, defined in system
        public TimeSpan GetOficiaIntervalHours(ApplicationUser user)
        {
            var oficialLunchHours = TimeSpan.Zero;
            if (user.StartLunchTime != null && user.EndLunchTime != null)
            {
                oficialLunchHours = ((DateTime)user.EndLunchTime).TimeOfDay - ((DateTime)user.StartLunchTime).TimeOfDay;
            }

            return oficialLunchHours;
        }

        //data: [{
        //x: '01/02/2019', <- Date of Saterday or Sunday
        //y: 8 <- Total Working Hours of User
        //},
        //... ]
        //Could be zero or multiple users
        public string GetJsonDataOfWeekendsDaysForGrafic(DateTime startDate, DateTime endDate, ApplicationUser user)
        {
            var jsonList = new List<JsonObj>();
            double totalWorkHours = 0;

            //if user is null -> get records for all users -> make 8 hours standard
            // if user -> get total working hours
            if (user != null)
            {
                var oficialWorkHours = GetOficialWorkHours(user);
                //transform Timespan for hours for grafic i.e. 2.3h
                totalWorkHours = ((oficialWorkHours.Hours * 60 + oficialWorkHours.Minutes) * 60 + oficialWorkHours.Seconds) / 3600.0;
            }
            else
            {
                totalWorkHours = 8;
            }

            while (startDate.Date <= endDate.Date)
            {
                //add to list just weekends
                if (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    //create serialized string of json for grafic
                    var thisJsonObj = new JsonObj() { x = startDate.Date.ToShortDateString(), y = totalWorkHours };
                    jsonList.Add(thisJsonObj);
                }
                startDate = startDate.AddDays(1);
            }
            //serialize list to Json string and pass to Controller and by ViewData pass to Chart.js
            var serializedList = JsonSerializer.Serialize(jsonList);

            return serializedList;
        }

        //data: [{
        //x: '01/02/2019', <- Date of Saterday or Sunday
        //y: 8 <- Total Working Hours of User
        //},
        //... ]
        //Could be zero or multiple users
        public string GetJsonDataOfBankHolidaysForGrafic(DateTime startDate, DateTime endDate, ApplicationUser user)
        {
            var jsonList = new List<JsonObj>();
            double totalWorkHours = 0;

            //if user is null -> get records for all users -> make 8 hours standard
            // if user -> get total working hours
            if (user != null)
            {
                var oficialWorkHours = GetOficialWorkHours(user);
                //transform Timespan for hours for grafic i.e. 2.3h
                totalWorkHours = ((oficialWorkHours.Hours * 60 + oficialWorkHours.Minutes) * 60 + oficialWorkHours.Seconds) / 3600.0;
            }
            else
            {
                totalWorkHours = 8;
            }

            var holidaysList = _context.BankHolidays.Where(c => c.Data.Date >= startDate.Date && c.Data.Date <= endDate.Date).ToList();

            foreach (var holiday in holidaysList)
            {
                //add to list bank holidays
                //create serialized string of json for grafic
                var thisJsonObj = new JsonObj() { x = holiday.Data.Date.ToShortDateString(), y = totalWorkHours };
                jsonList.Add(thisJsonObj);

            }
            //serialize list to Json string and pass to Controller and by ViewData pass to Chart.js
            var serializedList = JsonSerializer.Serialize(jsonList);

            return serializedList;
        }

        //data: [{
        //x: '01/02/2019', <- Date
        //y: 8 <- Total Working Hours of User
        //},
        //... ]

        // if user!=null -> return json string with holidays dates of concrete user in specified period
        public string GetJsonDataOfUserHolidaysForGrafic(DateTime startDate, DateTime endDate, ApplicationUser user)
        {
            var jsonList = new List<JsonObj>();

            if (user != null)
            {
                //get total working hours
                var oficialWorkHours = GetOficialWorkHours(user);
                //transform Timespan for hours for grafic i.e. 2.3h
                double totalWorkHours = ((oficialWorkHours.Hours * 60 + oficialWorkHours.Minutes) * 60 + oficialWorkHours.Seconds) / 3600.0;

                var holidaysList = _context.UserHolidays.Where(c => c.UserId == user.Id && c.HolidayDay.Date >= startDate.Date && c.HolidayDay.Date <= endDate.Date).ToList();

                foreach (var holiday in holidaysList)
                {
                    //add to list bank holidays
                    //create serialized string of json for grafic
                    var thisJsonObj = new JsonObj() { x = holiday.HolidayDay.Date.ToShortDateString(), y = totalWorkHours };
                    jsonList.Add(thisJsonObj);

                }
            }
            //serialize list to Json string and pass to Controller and by ViewData pass to Chart.js
            var serializedList = JsonSerializer.Serialize(jsonList);

            return serializedList;
        }


        public class JsonObj1
        {
            public string x { get; set; }
            public int y { get; set; }
        }

        //data: [{
        //x: '01/02/2019', <- Date
        //y: 1 <- N Auditorias, Resulyados, or Consultas
        //},
        //... ]
        //Could be zero or multiple users
        // 1 -> Auditorias
        // 2 -> Consultas
        // 3 -> Relatorios
        public string GetJsonDataForGraficResults(List<Result> listOfResults, int resultTypeId)
        {
            var jsonList = new List<JsonObj1>();

            //get entries of given type: Auditorias, Consultas or Relatorios
            var resultOfTheTypeList = listOfResults.Where(c => c.ResultTypeId == resultTypeId).ToList();

            foreach (var result in resultOfTheTypeList)
            {

                var thisJsonObj = new JsonObj1() { x = result.Data.ToShortDateString(), y = result.NumberOfResults };
                jsonList.Add(thisJsonObj);
            }

            //serialize list to Json string and pass to Controller and by ViewData pass to Chart.js
            var serializedList = JsonSerializer.Serialize(jsonList);

            return serializedList;
        }



        public TimeSpan TotalWorkHoursFromRecordsList(List<ReportsViewModel> listOfRecords)
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

            var newTime = new TimeSpan(hours, minuts, seconds);
            //to string time format ex 9 -> 09
            //string hoursStr = hours > 9 ? hours.ToString() : String.Concat("0", hours);
            //string minutsStr = minuts > 9 ? minuts.ToString() : String.Concat("0", minuts);
            //string secondsStr = seconds > 9 ? seconds.ToString() : String.Concat("0", seconds);

            //return String.Concat(hoursStr, ":", minutsStr, ":", secondsStr);

            return newTime;
        }

        public TimeSpan TotalIntervalsHoursFromRecordsList(List<ReportsViewModel> listOfRecords)
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

            var newTime = new TimeSpan(hours, minuts, seconds);

            return newTime;
        }

        //user!=null
        //all records pertence to one user
        //return list of records with Saturday, Sunday, Bank and User Holidays
        public List<ReportsViewModel> GetListofRecordsWithWeekendsUserAndBankHolidays(List<ReportsViewModel> listOfRecords,
            ApplicationUser user, DateTime startDate, DateTime endDate)
        {
            var bankHolidays = _context.BankHolidays.Include(c=>c.BankHolidaysType).ToList();
            var userHolidays = _context.UserHolidays.Where(c => c.UserId == user.Id).ToList();

            var listOfRecordsWithHolidays = new List<ReportsViewModel>();
            //Insert here the int of day tipe, to show in table and reports:
            //1 - Ferias
            //2 - Fim-de-Semana
            //3 - Feriado
            //4 - Complite Working Day
            //5 - Incomplite Working Day
            while(startDate.Date <= endDate.Date)
            {
                var recordWithStartDate = listOfRecords.FirstOrDefault(c => c.Data.Date == startDate.Date);

                //if listOfRecords do not has a record with StartDate
                if (recordWithStartDate==null)
                {
                    var holidayType = 0;
                    var isUserHoliday = userHolidays.FirstOrDefault(c => c.HolidayDay.Date == startDate.Date);
                    var isBankHoliday = bankHolidays.FirstOrDefault(c => c.Data.Date == startDate.Date);

                    //Is StartDate User Holidays?
                    if (isUserHoliday != null)
                    {
                        holidayType = 1;
                    }
                    //Is it StartDate Bank Holiday?
                    else if (isBankHoliday != null)
                    {
                        holidayType = 3;
                    }
                    //Is StartDate Weekend?
                    else if (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        holidayType = 2;
                    }
                    

                    //if StartDate is one of: Weekend, User or Bank Holiday
                    //then add new record with ReportDayType = holidayType
                    if (holidayType != 0)
                    {
                        var reportRecord = new ReportsViewModel()
                        {
                            Data = startDate.Date,
                            User = "",
                            UserDepartment = "",
                            UserOccupation = "",
                            DayStartTime = DateTime.Now,
                            StartDayDelayExplanation = "",
                            DayEndTime = DateTime.Now,
                            EndDayDelayExplanation = "",
                            TotalHoursForWork = "",
                            TotalHoursForIntervals = "",
                            ReportDayType = holidayType,
                            IsBankHolydaysName = holidayType == 3 ? isBankHoliday.BankHolidaysType.BankHolidayTypeName : ""
                        };

                        listOfRecordsWithHolidays.Add(reportRecord);
                    }
                    startDate = startDate.AddDays(1);
                }//end if
                //if listOfRecords has a record with StartDate
                else
                {
                    var totalWorkHoursPorDay = GetTotalWorkHoursPorDay(recordWithStartDate.DayRecordsId);
                    var oficialWorkingHoursOfUser = GetOficialWorkHours(user);
                    var diference = totalWorkHoursPorDay - oficialWorkingHoursOfUser;
                    if (((TimeSpan)diference).TotalHours > 0)
                    {
                        recordWithStartDate.ReportDayType = 4;
                    }
                    else
                    {
                        recordWithStartDate.ReportDayType = 5;
                    }

                    recordWithStartDate.IsBankHolydaysName = "";
                    listOfRecordsWithHolidays.Add(recordWithStartDate);
                    startDate = startDate.AddDays(1);
                }
            }//end while
            return listOfRecordsWithHolidays;
        }
    }
}
