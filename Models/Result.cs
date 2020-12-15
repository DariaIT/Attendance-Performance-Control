using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public class Result
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Data")]
        public DateTime Data { get; set; }

        [Required]
        [Display(Name = "Número")]
        public int NumberOfResults { get; set; }

        [Required]
        [Display(Name = "Tipo de Resultado")]
        public int ResultTypeId { get; set; }
        public virtual ResultType Type { get; set; }

        [Required]
        [Display(Name = "Nome")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

    }
}
