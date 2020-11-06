using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Attendance_Performance_Control.Data;
using Attendance_Performance_Control.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Attendance_Performance_Control.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "O campo Primeiro Nome é obrigatório.")]
            [Display(Name = "Primeiro Nome")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "O campo Segundo Nome é obrigatório.")]
            [Display(Name = "Segundo Nome")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "O campo Cargo é obrigatório.")]
            [Display(Name = "Cargo")]
            public int OccupationId { get; set; }

            [Phone(ErrorMessage = "O valor fornecido não é válido para número de telemóvel")]
            [Display(Name = "Telemóvel")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                OccupationId = user.OccupationId,
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível carregar utilizador com o ID '{_userManager.GetUserId(User)}'.");
            }

            var occupationsList = await _context.Occupations.Where(c => c.Id != 6).OrderBy(c => c.OccupationName).ToListAsync();
            ViewData["Occupations"] = new SelectList(occupationsList, "Id", "OccupationName");

            await LoadAsync(user);

            //Pass info to view if user is in role of admin
            ViewData["IsInRoleAdmin"] = await _userManager.IsInRoleAsync(user, "Admin");

            //Get Admin Occupation Name
            ViewData["AdminOccupName"] = _context.Occupations.FirstOrDefault(c => c.Id == 6).OccupationName;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível carregar utilizador com o ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Erro inesperado ao tentar definir o número de telefone.";
                    return RedirectToPage();
                }
            }

            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
            }

            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
            }

            if (Input.OccupationId != user.OccupationId)
            {
                user.OccupationId = Input.OccupationId;
            }

            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "O seu perfil foi atualizado.";
            return RedirectToPage();
        }
    }
}
