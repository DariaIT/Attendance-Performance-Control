﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name ="Primeiro Nome")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Segundo Nome")]
        public string LastName { get; set; }

        [Display(Name ="Hora do Início do trabalho")]
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

        [Required]
        [Display(Name = "Cargo")]
        public int OccupationId { get; set; }
        public virtual Occupation Occupation {get; set;}

        public string FullName => $"{FirstName} {LastName}";
    }
}