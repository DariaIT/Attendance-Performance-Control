using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attendance_Performance_Control.Data;
using Attendance_Performance_Control.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Attendance_Performance_Control.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            //in db.AspNetRoles find id of Admin Role
            var adminRoleId = await _context.Roles.Where(c => c.Name == "Admin").Select(c=>c.Id).SingleOrDefaultAsync();
            //in db.UserRoles find UserId by Admin Role Id
            var userAdminId = await _context.UserRoles.Where(c => c.RoleId == adminRoleId).Select(c=>c.UserId).SingleOrDefaultAsync();
            // in Identity find all users excluding admin to list
            var users = await _userManager.Users.Where(c=>c.Id != userAdminId).ToListAsync();
            return View(users);
        }


        //@confirm - false - show page confirmation
        //@confirm - true - confirm user's email
        [HttpPost]
        public async Task<IActionResult> Confirm(string Id, string confirm)
        {
            var user = _userManager.FindByIdAsync(Id).Result;
            if (user != null)
            {
                //if @confirm - true - confirm user's email
                if (String.Compare(confirm,"1")==0)
                {
                    try
                    {
                        user.EmailConfirmed = true;
                        await _context.SaveChangesAsync();
                        ModelState.AddModelError("confirmsuccess", "O registo de utilizador está confirmado e email de confirmação está enviado.");
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException)
                    {
                        ModelState.AddModelError(String.Empty, "Não foi possível salvar as alterações." +
                                                               "Tente novamente e se o problema persistir," +
                                                               "notifique o administrador do sistema.");
                    }
                    
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
