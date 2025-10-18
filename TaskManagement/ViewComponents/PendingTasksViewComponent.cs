using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagement.Data;
using TaskManagement.Models;
using System.Linq;
using System.Collections.Generic;

namespace TaskManagement.ViewComponents
{
    public class PendingTasksViewComponent : ViewComponent
    {
        private readonly TaskDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PendingTasksViewComponent(TaskDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync((ClaimsPrincipal)User);
            var pendingTasks = new List<AssignedTask>();
            if (user != null)
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
                if (employee != null)
                {
                    pendingTasks = await _context.AssignedTasks
                        .Where(t => t.UserId == employee.Id && t.Status == TaskManagement.Models.TaskStatus.Pending)
                        .Include(t => t.Task)
                        .ToListAsync();
                }
            }
            return View("Default", pendingTasks);
        }
    }
}
