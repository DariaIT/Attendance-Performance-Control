using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Models
{
    public partial class Department
    {
        public Department ()
        {
            Occupations = new HashSet<Occupation>();
        }

        public int Id { get; set; }

        [Required]
        [Display(Name = "Departamento")]
        public string DepartmentName { get; set; }


        public virtual ICollection<Occupation> Occupations { get; set; }
    }
}
