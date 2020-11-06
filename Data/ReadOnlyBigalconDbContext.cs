using Attendance_Performance_Control.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Attendance_Performance_Control.Data
{
    public class ReadOnlyBigalconDbContext: DbContext
    {
        public ReadOnlyBigalconDbContext(DbContextOptions<ReadOnlyBigalconDbContext> options)
           : base(options)
        {
        }

        public DbSet<contactosTaxas> contactosTaxas { get; set; }

        //for define Db as ReadOnly
        public override int SaveChanges()
        {
            // Throw if they try to call this
            throw new InvalidOperationException("This context is read-only.");
        }

    }
}
