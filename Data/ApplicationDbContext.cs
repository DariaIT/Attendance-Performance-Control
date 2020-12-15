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
        public DbSet<Department> Departments { get; set; }
        public DbSet<Occupation> Occupations { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<ResultType> ResultTypes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);


            //Department 1
            modelBuilder.Entity<Department>().HasData(
                new Department
                {
                    Id = 1,
                    DepartmentName = "Dep. SO"
                }
            );

            modelBuilder.Entity<Occupation>().HasData(
               new Occupation { Id = 1, OccupationName = "Coordenadora Dep. Saúde", DepartmentId = 1 },
               new Occupation { Id = 2, OccupationName = "Gestor Clientes Saúde", DepartmentId = 1 },
               new Occupation { Id = 3, OccupationName = "TDT (Técnico de Diagnóstico e Terapêutica)", DepartmentId = 1 },
               new Occupation { Id = 4, OccupationName = "Enfermeira", DepartmentId = 1 }
           );


            //Department 2
            modelBuilder.Entity<Department>().HasData(
               new Department
               {
                   Id = 2,
                   DepartmentName = "Dep. Formação"
               }
           );

            modelBuilder.Entity<Occupation>().HasData(
               new Occupation { Id = 5, OccupationName = "Gestor de Formação", DepartmentId = 2 }
           );


            //Department 3
            modelBuilder.Entity<Department>().HasData(
                new Department
                {
                    Id = 3,
                    DepartmentName = "Dep. Admin"
                }
            );

            modelBuilder.Entity<Occupation>().HasData(
               new Occupation { Id = 6, OccupationName = "Diretor Geral", DepartmentId = 3 },
               new Occupation { Id = 7, OccupationName = "Assessora Financeira", DepartmentId = 3 },
               new Occupation { Id = 8, OccupationName = "Técnico Administrativo", DepartmentId = 3 },
               new Occupation { Id = 9, OccupationName = "Empregada de Limpeza", DepartmentId = 3 }
           );


            //Department 4
            modelBuilder.Entity<Department>().HasData(
               new Department
               {
                   Id = 4,
                   DepartmentName = "Dep. Comercial"
               }
           );

            modelBuilder.Entity<Occupation>().HasData(
               new Occupation { Id = 10, OccupationName = "Técnico Comercial", DepartmentId = 4 }
           );

            //Department 5
            modelBuilder.Entity<Department>().HasData(
                new Department
                {
                    Id = 5,
                    DepartmentName = "Dep. HST"
                }
            );

            modelBuilder.Entity<Occupation>().HasData(
               new Occupation { Id = 11, OccupationName = "TSST", DepartmentId = 5 }
           );


            //create Results Types

            modelBuilder.Entity<ResultType>().HasData(
                new ResultType
                {
                    Id = 1,
                    ResultTypeName = "Auditoria"
                },
                new ResultType
                {
                    Id = 2,
                    ResultTypeName = "Consulta"
                },
                new ResultType
                {
                    Id = 3,
                    ResultTypeName = "Relatório"
                }
                );

        }
    }
}
