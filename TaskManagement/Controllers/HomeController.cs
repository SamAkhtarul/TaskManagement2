using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;
using TaskManagement.ViewModels;

namespace TaskManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TaskDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public HomeController(ILogger<HomeController> logger, TaskDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity is { IsAuthenticated: true })
            {
                var identityUser = await _userManager.GetUserAsync(User);
                if (identityUser != null)
                {
                    ViewData["Username"] = identityUser.UserName;

                    var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == identityUser.Email);

                    if (employee != null)
                    {
                        var tasks = _context.AssignedTasks.Where(t => t.UserId == employee.Id).Include(t => t.Task);

                        var dashboardViewModel = new DashboardViewModel
                        {
                            TotalTasks = await tasks.CountAsync(),
                            CompletedTasks = await tasks.CountAsync(t => t.Status == Models.TaskStatus.Completed),
                            PendingTasks = await tasks.CountAsync(t => t.Status == Models.TaskStatus.Pending),
                            InProgressTasks = await tasks.CountAsync(t => t.Status == Models.TaskStatus.InProgress),
                            OverdueTasks = await tasks.CountAsync(t => t.Status == Models.TaskStatus.Overdue),
                            RecentTasks = await tasks.Where(t => t.Status == Models.TaskStatus.Pending).OrderByDescending(t => t.AssignedDate).Take(5).ToListAsync()
                        };

                        return View(dashboardViewModel);
                    }
                    else
                    {
                        // User is authenticated but not an employee.
                        return View(new DashboardViewModel());
                    }
                }
                else
                {
                    // User is authenticated but not found in user manager.
                    return View(new DashboardViewModel());
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
