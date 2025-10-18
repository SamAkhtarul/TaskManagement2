using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TaskManagement.Controllers
{
    public class ManageRoleController : Controller
    {
        private RoleManager<IdentityRole> _roleManager;
        public ManageRoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult Index()
        {
            // This method will return a view that lists all roles
            var roles = _roleManager.Roles.OrderBy(r=>r.Name).ToList();
            return View(roles);
        }
        [Authorize(Roles = "Super Admin")]
        public IActionResult Create()
        {
            // This method will return a view to create a new role
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> Create(string roleName)
        {
            // This method will return a view to create a new role
         var result= await   _roleManager.CreateAsync(new IdentityRole(roleName));
            if(result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                string msg= "";
                foreach (var error in result.Errors)
                {
                 //  msg += error.Code +  error.Description+ "\n";
                    msg +=$"{error.Code} - {error.Description} \n";
                }
         ViewBag.Msg=  msg ;
            }
            return View();
        }

        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> Edit(string id, string roleName)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            role.Name = roleName;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(role);
        }

        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(role);
        }
    }
}
