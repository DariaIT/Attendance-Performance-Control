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

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:t}")]
        [Display(Name = "Início")]
        public DateTime DayStartTime { get; set; }

        [Display(Name = "Explicação do atraso")]
        public string StartDayDelayExplanation { get; set; }

        public bool StartDayDelayFlag { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:t}")]
        [Display(Name = "Fim")]
        public DateTime? DayEndTime { get; set; }

        [Display(Name = "Explicação do atraso")]
        public string EndDayDelayExplanation { get; set; }

        public bool EndDayDelayFlag { get; set; }
        public List<IntervalRecord> IntervalsList { get; set; }
        
        public string TotalHoursPorDay { get; set; }
    }

    
}
