using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskManagement.Data;
using TaskManagement.Models;
using System.IO;
using System.Linq;

namespace TaskManagement.Controllers
{
   [Authorize(Roles = "Admin,Super Admin")]
    public class UserController : Controller
    {
        private readonly TaskDbContext _dbContext;
        private UserManager<IdentityUser> _userManager;
        public UserController(TaskDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public IActionResult Index()
        
        {
            if (User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name) && User.IsInRole("Admin,Super Admin,Employee"))
            {
                var employees = _dbContext.Employees.Where(e => e.Email.Equals(User.Identity.Name)).ToList();
                return View(employees);
            }
            var users = _dbContext.Employees.ToList();
            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Employee user)
        {
            if (ModelState.IsValid)
            {
                if (user.Picture != null)
                {
                    _dbContext.Employees.Add(user);
                    _dbContext.SaveChanges();

                    string fileName = Path.GetFileName(user.Picture.FileName);
                    string extension = Path.GetExtension(fileName).ToLowerInvariant();
                    string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pictures");
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }
                    string filePath = Path.Combine(dirPath, user.Id + extension);
                    if (extension != ".jpg" && extension != ".png" && extension != ".jpeg")
                    {
                        ModelState.AddModelError("Picture", "Only .jpg, .png, and .jpeg files are allowed.");
                        _dbContext.Employees.Remove(user);
                        _dbContext.SaveChanges();
                        return View(user);
                    }
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        user.Picture.CopyTo(stream);
                    }
                    user.PicturePath = "/Pictures/" + user.Id + extension;
                    _dbContext.Employees.Update(user);
                    if (_dbContext.SaveChanges() > 0)
                    {
                        string password = $"{user.Name.ToUpper().Substring(0, 3)}*566#p";
                        var identityUser = new IdentityUser
                        {
                            UserName = user.Email,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNo
                        };
                        var result = await _userManager.CreateAsync(identityUser, password);
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(identityUser, "Employee");
                        }
                        else
                        {
                            string msg = string.Join(" | ", result.Errors.Select(e => $"{e.Code} - {e.Description}"));
                            ModelState.AddModelError("", msg);
                            return View(user);
                        }
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    _dbContext.Employees.Add(user);
                    if (_dbContext.SaveChanges() > 0)
                    {
                        string password = $"{user.Name.ToUpper().Substring(0, 3)}*566#p";
                        var identityUser = new IdentityUser
                        {
                            UserName = user.Email,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNo
                        };
                        var result = await _userManager.CreateAsync(identityUser, password);
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(identityUser, "Employee");
                        }
                        else
                        {
                            string msg = string.Join(" | ", result.Errors.Select(e => $"{e.Code} - {e.Description}"));
                            ModelState.AddModelError("", msg);
                            return View(user);
                        }
                        return RedirectToAction("Index");
                    }
                }
                ModelState.AddModelError("", "Failed to create user. Please try again.");
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                ModelState.AddModelError(" ", message);
            }
            return View(user);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _dbContext.Employees.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(Employee user)
        {
            var emp = _dbContext.Employees.FirstOrDefault(e => e.Id.Equals(user.Id));
            if (emp == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                if (user.Picture != null)
                {
                    string fileName = Path.GetFileName(user.Picture.FileName);
                    string extension = Path.GetExtension(fileName).ToLowerInvariant();
                    if(emp.PicturePath != null){
                        string oldPath = emp.PicturePath;
                        string oldPathCombine = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldPathCombine))
                        {
                            System.IO.File.Delete(oldPathCombine);
                        }
                    }

                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pictures", user.Id + extension);
                    if (extension != ".jpg" && extension != ".png" && extension != ".jpeg")
                    {
                        ModelState.AddModelError("Picture", "Only .jpg, .png, and .jpeg files are allowed.");
                        return View(user);
                    }
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        user.Picture.CopyTo(stream);
                    }
                    emp.PicturePath = "/Pictures/" + user.Id + extension;
                }
                emp.Name = user.Name;
                emp.PhoneNo = user.PhoneNo;
                _dbContext.Employees.Update(emp);
                if (_dbContext.SaveChanges() > 0)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Failed to update user. Please try again.");
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                ModelState.AddModelError(" ", message);
            }
            return View(user);
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = _dbContext.Employees.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = _dbContext.Employees.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = _dbContext.Employees.Find(id);
            if (user == null) return NotFound();

            if (user.Email != null)
            {
                var identityUser = await _userManager.FindByEmailAsync(user.Email);
                if (identityUser != null)
                {
                    var result = await _userManager.DeleteAsync(identityUser);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", "Failed to delete user from identity.");
                        return View(user);
                    }
                }
            }

            if (!string.IsNullOrEmpty(user.PicturePath))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.PicturePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _dbContext.Employees.Remove(user);
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}