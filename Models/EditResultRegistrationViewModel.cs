using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public class EditResultRegistrationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Por favor, introduza o número.")]
        [Display(Name = "Número")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "O valor deve ser numérico, maior que zero.")]
        public string Number { get; set; }

        [Display(Name = "Tipo de Resultado")]
        public int ResultTypeId { get; set; }
    }
}
