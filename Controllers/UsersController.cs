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
            var users = await _userManager.Users.Where(c => c.Id != userAdminId).ToListAsync();
            return View(users);
        }


        [HttpGet]
        public async Task<IActionResult> Confirm(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user != null)
            {
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
        public async Task<IActionResult> ConfirmPost(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user != null)
            {
                try
                {
                    user.EmailConfirmed = true;
                    await _context.SaveChangesAsync();
                    //try to send email confirmation to registered user
                    try
                    {
                        await _emailSender.SendEmailAsync(user.Email, "Confirmação do Registo",
                            $"<h1>PreviaSafe</h1><br />Caro funcionário, o seu registo em aplicação 'Previa Safe' está confirmado por Administrador, pode começar a utilizar. Obrigado.");
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError(String.Empty, "Não foi possível enviar o email de confirmação." +
                                                               "Tente novamente e se o problema persistir," +
                                                               "notifique o administrador do sistema.");
                        return View(user);
                    }
                    TempData["Success"] =
                        "O registo de utilizador está confirmado e email de confirmação está enviado.";
                    //ModelState.AddModelError("confirmsuccess", "O registo de utilizador está confirmado e email de confirmação está enviado.");
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
                ModelState.AddModelError(String.Empty, "Utilizador não existe");
            }
            return View();
        }

    }
}
