using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public class BankHoliday
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime Data { get; set; }

        [Required]
        [Display(Name = "Tipo")]
        public int BankHolidaysTypeId { get; set; }

        public virtual BankHolidaysType BankHolidaysType { get; set; }
    }
}
