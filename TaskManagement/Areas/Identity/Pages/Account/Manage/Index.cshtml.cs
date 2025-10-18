using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly TaskDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            TaskDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public string? PicturePath { get; set; }

        public class InputModel
        {
            [Display(Name = "Full Name")]
            [StringLength(25, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            public string Name { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Profile Picture")]
            public IFormFile? Picture { get; set; }
        }


        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
            if (employee == null)
            {
                employee = new Employee { Email = user.Email, Name = userName ?? "" };
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
            }

            Username = userName;
            PicturePath = employee.PicturePath;

            Input = new InputModel
            {
                Name = employee.Name,
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
            if (employee == null)
            {
                return NotFound($"Unable to find employee record for user with email '{user.Email}'.");
            }

            // Update Name
            if (Input.Name != employee.Name)
            {
                employee.Name = Input.Name;
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();
            }

            // Update Phone Number
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // Handle Picture Upload
            if (Input.Picture != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.Picture.FileName);
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Pictures", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.Picture.CopyToAsync(stream);
                }

                employee.PicturePath = "/Pictures/" + fileName;
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
