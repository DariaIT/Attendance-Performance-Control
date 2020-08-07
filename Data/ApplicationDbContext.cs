using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Attendance_Performance_Control.Models;

namespace Attendance_Performance_Control.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DayRecord> DayRecords { get; set; }
        public DbSet<TimeRecord> TimeRecords { get; set; }
        public DbSet<IntervalRecord> IntervalRecords { get; set; }
    }
}
