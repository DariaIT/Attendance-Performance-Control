using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public class BankHolidaysType
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string BankHolidayTypeName { get; set; }

    }
}
