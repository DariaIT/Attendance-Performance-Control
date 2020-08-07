using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public class TimeRecord
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        //@ store time of record saving - DateTime.Now
        [Required]
        public DateTime RecordTime { get; set; }


        public int DayRecordId { get; set; }
        public virtual DayRecord DayRecord { get; set; }
    }
}
