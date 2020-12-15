using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public class ResultType
    {

        public int Id { get; set; }

        [Required]
        public string ResultTypeName { get; set; }

    }
}
