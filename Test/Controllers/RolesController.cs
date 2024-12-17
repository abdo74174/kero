using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utility;
using Models;
//using ViewModel;

namespace Test.Controllers
{
    [Authorize(Roles = SD.Admin)]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public RolesController( RoleManager<IdentityRole> roleManager)

        {
            this.roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var roles = await roleManager.Roles.ToListAsync();
            return View(roles);
        }
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> Add(RoleFormViewModel model)
    //    {
    //        if (!ModelState.IsValid) { 
    //            return View("Index" , await roleManager.Roles.ToListAsync());
    //        }
    //        var roleIsExsist = await roleManager.RoleExistsAsync(model.Name);
    //        if (roleIsExsist) {
    //            ModelState.AddModelError("Name", "Roles is exsist");
    //        }

    //        await roleManager.CreateAsync(new IdentityRole { Name = model.Name.Trim() });
    //        return RedirectToAction("index");
    //    }
    }


}
