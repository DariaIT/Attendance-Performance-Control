using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public class MinimumUserResults
    {
        public int Id { get; set; }

        [Required]
        [Display(Name ="Min. Auditorias")]
        public int MinimumAuditorias { get; set; }

        [Required]
        [Display(Name = "Min. Consultas")]
        public int MinimumConsultas { get; set; }

        [Required]
        [Display(Name = "Min. Relatórios")]
        public int MinimumRelatorios { get; set; }

        [Required]
        [Display(Name = "Nome")]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
