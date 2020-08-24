using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public enum IntervalTypes
    {
        [Display(Name = "Almoço")]
        Interval1,
        [Display(Name = "Cafe")]
        Interval2,
        [Display(Name = "Médico")]
        Interval3,
        [Display(Name = "Outro")]
        Interval4
    }
}
