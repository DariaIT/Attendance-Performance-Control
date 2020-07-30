﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public enum Occupations
    {
        [Display(Name = "Diretor Geral")]
        Occupation1,
        [Display(Name = "Assessora Financeira")]
        Occupation2,
        [Display(Name = "Coordenadora Dep. Saúde")]
        Occupation3,
        [Display(Name = "Gestor Clientes Saúde")]
        Occupation4,
        [Display(Name = "TSST")]
        Occupation5,
        [Display(Name = "Técnico Administrativo")]
        Occupation6,
        [Display(Name = "Técnico Comercial")]
        Occupation7,
        [Display(Name = "TDT (Técnico de Diagnóstico e Terapeutica)")]
        Occupation8,
        [Display(Name = "Gestor de Clientes")]
        Occupation9,
        [Display(Name = "Enfermeira")]
        Occupation10,
        [Display(Name = "Empregada de Limpeza")]
        Occupation11
    }
}
