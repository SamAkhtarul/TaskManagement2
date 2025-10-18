using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Controllers;

public class TaskAssignedController : Controller
{
    private readonly TaskDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;

    public TaskAssignedController(TaskDbContext dbContext, UserManager<IdentityUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [Authorize(Roles = "Admin,Super Admin,Employee")]
    public IActionResult Index(string sortOrder, string filterStatus, string searchString)
    {
        ViewData["CurrentSort"] = sortOrder;
        ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["TaskSortParm"] = sortOrder == "Task" ? "task_desc" : "Task";
        ViewData["DueDateSortParm"] = sortOrder == "DueDate" ? "duedate_desc" : "DueDate";
        ViewData["CurrentFilter"] = searchString;

        var assignedTasks = _dbContext.AssignedTasks.Include("Task").Include("User").AsQueryable();

        if (!String.IsNullOrEmpty(filterStatus))
        {
            if (Enum.TryParse<TaskManagement.Models.TaskStatus>(filterStatus, out var status))
            {
                assignedTasks = assignedTasks.Where(s => s.Status == status);
            }
        }

        if (!String.IsNullOrEmpty(searchString))
        {
            assignedTasks = assignedTasks.Where(s => s.Task.Title.Contains(searchString) || (s.Remarks != null && s.Remarks.Contains(searchString)));
        }

        switch (sortOrder)
        {
            case "name_desc":
                assignedTasks = assignedTasks.OrderByDescending(s => s.User.Name);
                break;
            case "Date":
                assignedTasks = assignedTasks.OrderBy(s => s.AssignedDate);
                break;
            case "date_desc":
                assignedTasks = assignedTasks.OrderByDescending(s => s.AssignedDate);
                break;
            case "Task":
                assignedTasks = assignedTasks.OrderBy(s => s.Task.Title);
                break;
            case "task_desc":
                assignedTasks = assignedTasks.OrderByDescending(s => s.Task.Title);
                break;
            case "DueDate":
                assignedTasks = assignedTasks.OrderBy(s => s.DueDate);
                break;
            case "duedate_desc":
                assignedTasks = assignedTasks.OrderByDescending(s => s.DueDate);
                break;
            default:
                assignedTasks = assignedTasks.OrderBy(s => s.User.Name);
                break;
        }

        return View(assignedTasks.ToList());
    }

    [Authorize(Roles = "Admin,Super Admin")]
    public async Task<IActionResult> Create()
    {
        var registeredUsers = await _userManager.Users.ToListAsync();
        var employees = new List<Employee>();
        foreach (var user in registeredUsers)
        {
            var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
            if (employee != null)
            {
                employees.Add(employee);
            }
        }

        var model = new AssignedTask
        {
            AssignedDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(7), // Default due date is 7 days from now
            Users = employees.OrderBy(u => u.Name).ToList(),
            Tasklist = await _dbContext.Tasks.OrderBy(t => t.Title).ToListAsync()
        };
        return View(model);
    }
    [HttpPost]
    [Authorize(Roles = "Admin,Super Admin")]
    public async Task<IActionResult> Create(AssignedTask assignedTask, List<int> Tasklist)
    {
        if (ModelState.IsValid)
        {
            int result = 0;
            foreach (var taskId in Tasklist)
            {
                var addnew= new AssignedTask
                {
                    UserId = assignedTask.UserId,
                    AssignedDate = assignedTask.AssignedDate,
                    DueDate = assignedTask.DueDate,
                    SubmitDate = assignedTask.SubmitDate,
                    Status = assignedTask.Status,
                    Remarks = assignedTask.Remarks
                };
                var task = await _dbContext.Tasks.FindAsync(taskId);
                if (task != null)
                {
                    addnew.Task = task;
                    addnew.TaskId = task.Id;
                }
                _dbContext.AssignedTasks.Add(addnew);
              
            }
            result = await _dbContext.SaveChangesAsync();
            if (result> 0)
            {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Failed to create assigned task. Please try again.");

        }
        else
        {
            var message = string.Join(" | ", ModelState.Values
    .SelectMany(v => v.Errors)
    .Select(e => e.ErrorMessage));
            ModelState.AddModelError(" ", message);
        }

        var registeredUsers = await _userManager.Users.ToListAsync();
        var employees = new List<Employee>();
        foreach (var user in registeredUsers)
        {
            var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
            if (employee != null)
            {
                employees.Add(employee);
            }
        }
        assignedTask.Users = employees.OrderBy(u => u.Name).ToList();
        assignedTask.Tasklist = await _dbContext.Tasks.OrderBy(t => t.Title).ToListAsync();
        return View(assignedTask);
    }

    [Authorize(Roles = "Admin,Super Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var assignedTask = await _dbContext.AssignedTasks.FindAsync(id);
        if (assignedTask == null)
        { 
            return NotFound();
        }
        assignedTask.Users = await _dbContext.Employees.OrderBy(u => u.Name).ToListAsync();
        assignedTask.Tasklist = await _dbContext.Tasks.OrderBy(t => t.Title).ToListAsync();
        return View(assignedTask);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Super Admin")]
    public async Task<IActionResult> Edit(AssignedTask assignedTask)
    {
        if (ModelState.IsValid)
        {
            _dbContext.AssignedTasks.Update(assignedTask);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        assignedTask.Users = await _dbContext.Employees.OrderBy(u => u.Name).ToListAsync();
        assignedTask.Tasklist = await _dbContext.Tasks.OrderBy(t => t.Title).ToListAsync();
        return View(assignedTask);
    }

    [Authorize(Roles = "Admin,Super Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var assignedTask = await _dbContext.AssignedTasks.Include(at => at.Task).Include(at => at.User).FirstOrDefaultAsync(at => at.Id == id);
        if (assignedTask == null)
        {
            return NotFound();
        }
        return View(assignedTask);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin,Super Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var assignedTask = await _dbContext.AssignedTasks.FindAsync(id);
        if (assignedTask == null)
        {
            return NotFound();
        }
        _dbContext.AssignedTasks.Remove(assignedTask);
        await _dbContext.SaveChangesAsync();
        return RedirectToAction("Index");
    }

}
