using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public class ReportsViewModel
    {
        public int DayRecordsId { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = "Data")]
        public DateTime Data { get; set; }

        [Display(Name = "Dep-to")]
        public string UserDepartment { get; set; }

        [Display(Name = "Cargo")]
        public string UserOccupation { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:t}")]
        [Display(Name = "Início")]
        public DateTime DayStartTime { get; set; }

        [Display(Name = "Explicação do atraso")]
        public string StartDayDelayExplanation { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:t}")]
        [Display(Name = "Fim")]
        public DateTime DayEndTime { get; set; }

        [Display(Name = "Explicação de saída prévia")]
        public string EndDayDelayExplanation { get; set; }

        [Display(Name = "Total de Trabalho (h)")]
        public string TotalHoursForWork { get; set; }

        [Display(Name = "Total de Intervalo (h)")]
        public string TotalHoursForIntervals { get; set; }
    }
}
