using DataAccess.Migrations;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Utility;

namespace Test.Controllers
{
    [Authorize(Roles = SD.Admin)]
    public class DiscountController : Controller
    {
        private readonly IDiscountRepository discountRepository;

        public DiscountController(IDiscountRepository discountRepository)
        {
            this.discountRepository = discountRepository;
        }

        public IActionResult Index()
        {
            var discounts = discountRepository.GetAll();
            foreach (var item in discounts)
            {
                if (item.StartDate < DateTime.Now && item.EndDate > DateTime.Now)
                {
                    if (item.CouponCounter > 0)
                    {
                        item.Status = DiscountStatus.valid;
                    }
                    else
                    {
                        item.Status = DiscountStatus.expires;
                    }
                }
                else
                {
                    item.Status = DiscountStatus.pending;
                }
            }

           
            

            return View(discounts);
        }

        public IActionResult Details(int id)
        {
            var discount = discountRepository.GetOne(expression: e => e.Id == id);
            if (discount == null)
            {
                TempData["ErrorMessage"] = "Discount not found.";
                return RedirectToAction("NotFoundPage", "Home");
            }
            return View(discount);
        }

        public IActionResult Create()
        {
            Discount discount = new Discount();
            return View(discount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Discount discount)
        {
            if (ModelState.IsValid)
            {
                discountRepository.Add(discount);
                TempData["SuccessMessage"] = "Discount created successfully!";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Error creating discount.";
            return View(discount);
        }

        public IActionResult Edit(int id)
        {
            var discount = discountRepository.GetOne(expression: e => e.Id == id);
            if (discount == null)
            {
                TempData["ErrorMessage"] = "Discount not found.";
                return RedirectToAction("NotFoundPage", "Home");
            }
            return View(discount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Discount discount)
        {
            if (ModelState.IsValid)
            {
                discountRepository.Update(discount);
                TempData["SuccessMessage"] = "Discount updated successfully!";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Error updating discount.";
            return View(discount);
        }

        public IActionResult Delete(int id)
        {
            Discount discount = new Discount() { Id = id };
            try
            {
                discountRepository.Delete(discount);
                TempData["SuccessMessage"] = "Discount deleted successfully!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error deleting discount.";
            }
            return RedirectToAction("Index");
        }
    }
}
