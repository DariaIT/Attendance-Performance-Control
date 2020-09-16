using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public class IntervalRecord
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public IntervalTypes? IntervalType { get; set; }

        public int DayRecordId { get; set; }
        public virtual DayRecord DayRecord { get; set; }
    }
}
