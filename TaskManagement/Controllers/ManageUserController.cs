using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskManagement.ViewModels;

namespace TaskManagement.Controllers
{
    [Authorize(Roles = "Admin,Super Admin")]
    public class ManageUserController : Controller
    {
        private UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public ManageUserController(UserManager<IdentityUser> userManager,RoleManager<IdentityRole>roleManager) {
            this._userManager = userManager;
            this._roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var userList = _userManager.Users.OrderBy(u => u.UserName).ToList();
            var userVM = new List<UserVM>();
            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userVM.Add(new UserVM
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    RoleName = string.Join(",", roles.ToList())
                });
            }

            return View(userVM);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserVM userVM)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = userVM.UserName, Email = userVM.Email };
                var result = await _userManager.CreateAsync(user, userVM.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(userVM);
        }

        public async Task<IActionResult> AssignRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null) 
            {
                ViewBag.Username = user.UserName;
                ViewBag.UserId = user.Id;
            }
            var role = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            ViewBag.RoleList = role;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, List<string> roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                try
                {
                    var existingRoles = await _userManager.GetRolesAsync(user);
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);

                    if (!removeResult.Succeeded)
                    {
                        string msg = "";
                        foreach (var error in removeResult.Errors)
                        {
                            msg += $"{error.Code} - {error.Description} \n";
                        }
                        ViewBag.Msg = msg;
                        return View();
                    }

                    var result = await _userManager.AddToRolesAsync(user, roleName);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        string msg = "";
                        foreach (var error in result.Errors)
                        {
                            msg += $"{error.Code} - {error.Description} \n";
                        }
                        ViewBag.Msg = msg;
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Msg = "An error occurred while assigning roles: " + ex.Message;
                }
            }
            else
            {
                ViewBag.Msg = "User not found.";
            }

            if (user != null)
            {
                ViewBag.Username = user.UserName;
            }
            var role = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            ViewBag.RoleList = role;
            return View();
        }//assign

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(user);
        }
    }//calss
}//ns
