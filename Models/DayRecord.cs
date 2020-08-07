using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public partial class DayRecord
    {
        public DayRecord()
        {
            TimeRecords = new HashSet<TimeRecord>();
            IntervalRecords = new HashSet<IntervalRecord>();
        }

        public int Id { get; set; }

        [Required]
        public DateTime Data { get; set; }

        //Identity User
        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<TimeRecord> TimeRecords { get; set; }
        public virtual ICollection<IntervalRecord> IntervalRecords { get; set; }
    }
}
