using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public class EditUserViewModel
    {
        [Required]
        public string Id {get; set;}

        [Required]
        [Display(Name = "Primeiro Nome")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Segundo Nome")]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Display(Name = "Departamento")]
        public string DepartmentName { get; set; }

        [Required]
        [Display(Name = "Cargo")]
        public int OccupationId { get; set; }
        public SelectList OccupationsList { get; set; }

        [Display(Name = "Hora do Início do trabalho")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:t}")]
        //[RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$",
        // ErrorMessage = "A data não é valida.")]
        public DateTime? StartWorkTime { get; set; }

        [Display(Name = "Hora do Fim do trabalho")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:t}")]
        //[RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$",
        // ErrorMessage = "A data não é valida.")]
        public DateTime? EndWorkTime { get; set; }

        [Display(Name = "Hora do Início do Almoço")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:t}")]
        //[RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$",
        // ErrorMessage = "A data não é valida.")]
        public DateTime? StartLunchTime { get; set; }

        [Display(Name = "Hora do Fim do Almoço")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:t}")]
        //[RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$",
        // ErrorMessage = "A data não é valida.")]
        public DateTime? EndLunchTime { get; set; }

        [Display(Name = "Ativar/Desativar Utilizador")]
        public bool IsActive { get; set; } 
    }
}
