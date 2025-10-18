using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Controllers;
[Authorize(Roles = "Admin,Super Admin")]
public class TaskListController : Controller
{
    private readonly TaskDbContext _dbContext;

    public TaskListController(TaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public IActionResult Index()
    {
        var tasks = _dbContext.Tasks.ToList();
        return View(tasks);
    }
    public IActionResult Create()
    {
         
        return View();
    }
    [HttpPost]
    public IActionResult Create(TaskList task)
    {
        if (ModelState.IsValid)
        {
            _dbContext.Tasks.Add(task);
            if (_dbContext.SaveChanges() > 0)
            {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Failed to create task. Please try again.");
        }
        return View(task);
    }
   
    [HttpGet]
    public IActionResult Get()
    {

        var tasks = _dbContext.Tasks.ToList();
        return Json(tasks);
    }
    public IActionResult GetbyId(int Id)
    {

        var tasks = _dbContext.Tasks.Find(Id);
        return Json(tasks);
    }
    public async Task<IActionResult> Save(TaskList task)
    {

       await  _dbContext.Tasks.AddAsync(task);
        if(_dbContext.SaveChanges()>0)
        {
            return Json(new { data=task, msg="successfully added "});
        }
        else
        {
            return Json(new { data = task, msg = "Failed to add. " });
        }
    }
    public  IActionResult Update(TaskList task)
    {

         _dbContext.Tasks.Update(task);
        if (_dbContext.SaveChanges() > 0)
        {
            return Json(new { data = task, msg = "successfully Updated " });
        }
        else
        {
            return Json(new { data = task, msg = "Failed to Update. " });
        }
    }
    public IActionResult Delete(int id)
    {

      var obj=  _dbContext.Tasks.Find(id);
        if (obj == null)
        {
            return Json(new { msg = "Task not found." });
        }
        _dbContext.Tasks.Remove(obj);
        if (_dbContext.SaveChanges() > 0)
        {
            return Json(new {   msg = "successfully Delete " });
        }
        else
        {
            return Json(new { msg = "Failed to Delete. " });
        }
    }
}
