using DataAccess.Repository;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using NuGet.ProjectModel;
using Utility;

namespace Test.Controllers
{
    [Authorize(Roles = SD.Admin)]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository categoryRepo;

        public CategoryController(ICategoryRepository category)
        {
            this.categoryRepo = category;
        }

        public IActionResult Index()
        {
            var Categories = categoryRepo.GetAll();
            return View(Categories);
        }

        public IActionResult Create()
        {
            Category category = new Category();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                categoryRepo.Add(category);
                TempData["SuccessMessage"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Failed to create category. Please try again.";
            return View(category);
        }

        public IActionResult Edit(int id)
        {
            var Category = categoryRepo.GetOne([], e => e.Id == id);
            if (Category == null)
            {
                TempData["ErrorMessage"] = "Category not found!";
                return RedirectToAction("NotFoundPage", "Home");
            }
            return View(Category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                categoryRepo.Update(category);
                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Failed to update category. Please try again.";
            return View(category);
        }

        public IActionResult Delete(int id)
        {
            Category category = new Category()
            {
                Id = id
            };
            categoryRepo.Delete(category);
            TempData["SuccessMessage"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
