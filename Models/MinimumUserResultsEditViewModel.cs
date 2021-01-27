using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public class MinimumUserResultsEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Por favor, introduza o número.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "O valor têm de ser numérico.")]
        [Display(Name = "Min. Auditorias")]
        public string MinimumAuditorias { get; set; }

        [Required(ErrorMessage = "Por favor, introduza o número.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "O valor têm de ser numérico.")]
        [Display(Name = "Min. Consultas")]
        public string MinimumConsultas { get; set; }

        [Required(ErrorMessage = "Por favor, introduza o número.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "O valor têm de ser numérico.")]
        [Display(Name = "Min. Relatórios")]
        public string MinimumRelatorios { get; set; }

        [Required(ErrorMessage = "Por favor, introduza o Nome")]
        [Display(Name = "Nome")]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
