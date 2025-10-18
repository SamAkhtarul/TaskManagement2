using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Data;

namespace TaskManagement.ViewComponents
{
    public class ProfilePictureViewComponent : ViewComponent
    {
        private readonly TaskDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfilePictureViewComponent(TaskDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync((ClaimsPrincipal)User);
            string picturePath = string.Empty;
            if (user != null)
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
                if (employee != null && !string.IsNullOrEmpty(employee.PicturePath))
                {
                    picturePath = employee.PicturePath;
                }
            }
            return View("Default", picturePath);
        }
    }
}
