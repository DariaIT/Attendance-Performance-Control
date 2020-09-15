using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Attendance_Performance_Control.Data;
using Attendance_Performance_Control.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Attendance_Performance_Control.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            //in db.AspNetRoles find id of Admin Role
            var adminRoleId = await _context.Roles.Where(c => c.Name == "Admin").Select(c => c.Id).SingleOrDefaultAsync();
            //in db.UserRoles find UserId by Admin Role Id
            var userAdminId = await _context.UserRoles.Where(c => c.RoleId == adminRoleId).Select(c => c.UserId).SingleOrDefaultAsync();
            // in Identity find all users excluding admin to list
            var users = await _userManager.Users.Where(c => c.Id != userAdminId).Include(c => c.Occupation).Include(d => d.Occupation.Department).ToListAsync();

            return View(users);
        }


        [HttpGet]
        public async Task<IActionResult> Confirm(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user != null)
            {
                var cargo = await _context.Occupations.FirstOrDefaultAsync(c => c.Id == user.OccupationId);
                var depart = await _context.Departments.FirstOrDefaultAsync(d => d.Id == cargo.DepartmentId);

                ViewBag.cargo = cargo.OccupationName;
                ViewBag.depart = depart.DepartmentName;
                return View(user);
            }
            else
            {
                ModelState.AddModelError(String.Empty, "Utilizador não existe");
            }
            return View();
        }


        [HttpPost]
        [ActionName("Confirm")]
        public async Task<IActionResult> ConfirmPost([Bind("Id,FirstName,LastName,StartWorkTime,EndWorkTime,StartLunchTime,EndLunchTime")] ApplicationUser applicationUser)
        {
            var user = await _userManager.FindByIdAsync(applicationUser.Id);

            if (user != null)
            {
                var cargo = await _context.Occupations.FirstOrDefaultAsync(c => c.Id == user.OccupationId);
                var depart = await _context.Departments.FirstOrDefaultAsync(d => d.Id == cargo.DepartmentId);

                ViewBag.cargo = cargo.OccupationName;
                ViewBag.depart = depart.DepartmentName;

                if (applicationUser.StartWorkTime.HasValue && applicationUser.EndWorkTime.HasValue)
                {
                    try
                    {
                        user.EmailConfirmed = true;
                        user.StartWorkTime = applicationUser.StartWorkTime;
                        user.EndWorkTime = applicationUser.EndWorkTime;
                        if (applicationUser.StartLunchTime.HasValue)
                            user.StartLunchTime = applicationUser.StartLunchTime;
                        if (applicationUser.EndLunchTime.HasValue)
                            user.EndLunchTime = applicationUser.EndLunchTime;
                        await _context.SaveChangesAsync();

                        //try to send email confirmation to registered user
                        try
                        {
                            await _emailSender.SendEmailAsync(user.Email, "Confirmação do Registo",
                                $"<h1>PreviaSafe</h1><br />Caro funcionário, o seu registo em aplicação 'Previa Safe' está confirmado por Administrador, pode começar a utilizar. Obrigado.");
                        }
                        catch (Exception)
                        {
                            ModelState.AddModelError(String.Empty, "Não foi possível enviar o email de confirmação." +
                                                                   "Tente novamente e se o problema persistir," +
                                                                   "notifique o administrador do sistema.");
                            return View(user);
                        }
                        //this message is showen in Index after redirect success
                        TempData["Success"] =
                            "O registo de utilizador está confirmado e o email de confirmação está enviado.";
                        return RedirectToAction(nameof(Index));

                    }
                    catch (DbUpdateException)
                    {
                        ModelState.AddModelError(String.Empty, "Não foi possível salvar as alterações." +
                                                               "Tente novamente e se o problema persistir," +
                                                               "notifique o administrador do sistema.");
                    }
                    return View(user);
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Por favor, adiciona a hora de início e a hora de fim de trabalho do funcionario.");
                    return View(user);
                }
            }
            else
            {
                ModelState.AddModelError(String.Empty, "Utilizador não existe");
            }

            return View();
        }


        public async Task<IActionResult> Edit(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            //initialize EditUserViewModel

            //get Department Id
            var departId = _context.Occupations.FirstOrDefault(c => c.Id == user.OccupationId).DepartmentId;

            var userModel = new EditUserViewModel()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DepartmentName = _context.Departments.FirstOrDefault(c => c.Id == departId).DepartmentName,
                StartWorkTime = user.StartWorkTime,
                EndWorkTime = user.EndWorkTime,
                StartLunchTime = user.StartLunchTime,
                EndLunchTime = user.EndLunchTime,
                OccupationId = user.OccupationId,
                OccupationsList = new SelectList(_context.Occupations, "Id", "OccupationName", user.OccupationId),
                IsActive = IsActiveUser(user.Id)
            };

            return View(userModel);
        }

        // POST: Records/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,FirstName,LastName,Email,DepartmentName,OccupationId," +
            "OccupationsList,StartWorkTime,EndWorkTime,StartLunchTime,EndLunchTime, IsActive")] EditUserViewModel editedUser)
        {
            var applicationUser = _userManager.FindByIdAsync(editedUser.Id).Result;
            if (applicationUser == null)
            {
                return NotFound();
            }
          
            //check if just one time of lunch is defind - reject, has to be both defind or both null
            if (editedUser.StartLunchTime == null && editedUser.EndLunchTime != null ||
                editedUser.StartLunchTime != null && editedUser.EndLunchTime == null)
            {
                ModelState.AddModelError(String.Empty, "Por favor, escolha data de início e fim de almoço.");
            }
            else if (editedUser.StartLunchTime >= editedUser.EndLunchTime)
            {
                ModelState.AddModelError(String.Empty, "A data do início de almoço está menor ou igual que a data do fim de almoço!");
            }
            else
            {
                try
                {
                    if (applicationUser.OccupationId != editedUser.OccupationId)
                        applicationUser.OccupationId = editedUser.OccupationId;
                    if (applicationUser.StartWorkTime.Value.TimeOfDay != editedUser.StartWorkTime.Value.TimeOfDay)
                        applicationUser.StartWorkTime = editedUser.StartWorkTime;
                    if (applicationUser.EndWorkTime.Value.TimeOfDay != editedUser.EndWorkTime.Value.TimeOfDay)
                        applicationUser.EndWorkTime = editedUser.EndWorkTime;
                    if (editedUser.StartLunchTime != null && applicationUser.StartLunchTime == null ||
                        editedUser.StartLunchTime != null && applicationUser.StartLunchTime.Value.TimeOfDay != editedUser.StartLunchTime.Value.TimeOfDay)
                        applicationUser.StartLunchTime = editedUser.StartLunchTime;
                    if (editedUser.EndLunchTime != null && applicationUser.EndLunchTime == null ||
                        editedUser.EndLunchTime != null && applicationUser.EndLunchTime.Value.TimeOfDay != editedUser.EndLunchTime.Value.TimeOfDay)
                        applicationUser.EndLunchTime = editedUser.EndLunchTime;
                    await _context.SaveChangesAsync();

                    //Activate User
                    if (editedUser.IsActive && applicationUser.LockoutEnd.HasValue)
                    {
                        await _userManager.SetLockoutEndDateAsync(applicationUser, null);
                    }
                    //Desactivate User
                    else if (!editedUser.IsActive && !applicationUser.LockoutEnd.HasValue)
                    {
                       var lockoutDate = DateTime.Now.AddYears(20);
                       await _userManager.SetLockoutEndDateAsync(applicationUser, lockoutDate);
                    }

                    //this message is showen in Index after redirect success
                    TempData["Success"] =
                        "Os dados de utilizador estão modificados com sucesso.";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError(String.Empty, "Não foi possível salvar as alterações." +
                                                                   "Tente novamente e se o problema persistir," +
                                                                   "notifique o administrador do sistema.");
                }
            }
            editedUser.OccupationsList = new SelectList(_context.Occupations, "Id", "OccupationName", editedUser.OccupationId);

            return View(editedUser);
        }


        public bool IsActiveUser(string id)
        {
            var user = _userManager.FindByIdAsync(id).Result;

            return user.LockoutEnd == null ? true : false;
        }


        public async Task<IActionResult> Delete(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Records/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                await _userManager.DeleteAsync(user);

                TempData["Success"] = "Utilizador foi eliminado com sucesso.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(String.Empty, "Algo correu mal, por favor, tente novamente.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
