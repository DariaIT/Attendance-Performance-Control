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
    }
}
