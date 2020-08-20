using Attendance_Performance_Control.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Attendance_Performance_Control.Models
{
    public class UserRecordViewModel
    {
        //One date record
        public DateTime Data { get; set; }

        [Display(Name = "Início")]
        public DateTime DayStartTime { get; set; }

        [Display(Name = "Fim")]
        public DateTime? DayEndTime { get; set; }
        public List<IntervalRecord> IntervalsList { get; set; }
   
    }

    
}
