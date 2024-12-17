using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IOrderRepository orderRepository;

        public UserController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOrderRepository orderRepository
            )
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index()
        {
            var users = await userManager.Users.ToListAsync();
            var userList = new List<ApplicationUserVM>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userList.Add(new ApplicationUserVM
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Roles = roles
                });
            }

            return View(userList);
        }

        public async Task<IActionResult> Add()
        {
            var roles = await roleManager.Roles
                .Select(r => new RoleVM { RoleName = r.Name, IsSelected = false })
                .ToListAsync();

            var viewModel = new AddUserViewModel
            {
                Roles = roles
            };

            return View(viewModel);
        }
        public async Task<IActionResult> Search(string searchTerm)
        {
           
            var users = await userManager.Users.ToListAsync();

            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => u.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                         u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

         
            var userVMs = new List<ApplicationUserVM>();

            foreach (var user in users)
            {
               
                var userRoles = await userManager.GetRolesAsync(user);

              
                userVMs.Add(new ApplicationUserVM
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName, 
                    Email = user.Email,
                    Roles = userRoles.ToList() 
                });
            }

           
            ViewBag.SearchTerm = searchTerm;

         
            return View(userVMs);
        }


        [HttpPost]
        public async Task<IActionResult> Add(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();
                if (!selectedRoles.Any())
                {
                    ModelState.AddModelError("", "At least one role must be selected.");
                    return View(model);
                }
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    Address = model.Address
                };
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var roleResult = await userManager.AddToRolesAsync(user, selectedRoles);
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                    TempData["SuccessMessage"] = "User added successfully!";
                    return RedirectToAction("Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            if (model.Roles == null)
            {
                model.Roles = await roleManager.Roles
                    .Select(r => new RoleVM { RoleName = r.Name, IsSelected = false })
                    .ToListAsync();
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("NotFoundPage", "Home");

            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await roleManager.Roles.ToListAsync();
            var roleVMs = new List<RoleVM>();

            foreach (var role in roles)
            {
                var isSelected = await userManager.IsInRoleAsync(user, role.Name);
                roleVMs.Add(new RoleVM
                {
                    RoleName = role.Name,
                    IsSelected = isSelected
                });
            }

            var viewModel = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                Address = user.Address,
                Roles = roleVMs
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.UserName;
            user.Address = model.Address;

            var existingRoles = await userManager.GetRolesAsync(user);
            var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();

            var rolesToAdd = selectedRoles.Except(existingRoles).ToList();
            var rolesToRemove = existingRoles.Except(selectedRoles).ToList();

            if (rolesToAdd.Any())
            {
                var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }

            if (rolesToRemove.Any())
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User has been modified successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new DeleteUserViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return NotFound();

            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var result = await userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Delete", new DeleteUserViewModel { UserId = user.Id });
        }

        public async Task<IActionResult> ManageRoles(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"User not found for ID: {userId}");
                return RedirectToAction("NotFoundPage", "Home");
            }

            var roles = await roleManager.Roles.ToListAsync();
            var rolesVM = new List<RoleVM>();

            foreach (var role in roles)
            {
                var isSelected = await userManager.IsInRoleAsync(user, role.Name);
                rolesVM.Add(new RoleVM
                {
                    RoleName = role.Name,
                    IsSelected = isSelected
                });
            }

            var viewModel = new UserRolesVM
            {
                UserId = userId,
                UserName = user.UserName,
                Roles = rolesVM
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ManageRoles(UserRolesVM model)
        {
            if (model == null || string.IsNullOrEmpty(model.UserId) || model.Roles == null || model.Roles.All(r => !r.IsSelected))
            {
                TempData["ErrorMessage"] = "There was an issue with the roles data. Please try again.";
                return RedirectToAction("ManageRoles", new { userId = model?.UserId });
            }

            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return RedirectToAction("NotFoundPage", "Home");
            }

            var existingRoles = await userManager.GetRolesAsync(user);

            var selectedRole = model.Roles.FirstOrDefault(r => r.IsSelected)?.RoleName;

            if (string.IsNullOrEmpty(selectedRole))
            {
                TempData["ErrorMessage"] = "No role selected. Please choose a role.";
                return RedirectToAction("ManageRoles", new { userId = model.UserId });
            }

            await userManager.RemoveFromRolesAsync(user, existingRoles);
            await userManager.AddToRoleAsync(user, selectedRole);

            TempData["SuccessMessage"] = "Role updated successfully!";
            return RedirectToAction("index");
        }
        public IActionResult AllUserOrders(string UserId)
        {
            
            var orders = orderRepository.GetAll([e=> e.OrderItems , equals=> equals.Discount , equals=> equals.Payment , equals=> equals.User] , expression: e=> e.UserId == UserId);
            return View(orders);
        }
    }
}
