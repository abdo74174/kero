using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Utility;

namespace Test.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewRepository reviewRepository;
        private readonly IProductRepository productRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public ReviewController(IReviewRepository reviewRepository, IProductRepository productRepository, UserManager<ApplicationUser> userManager)
        {
            this.reviewRepository = reviewRepository;
            this.productRepository = productRepository;
            this.userManager = userManager;
        }
        [Authorize(Roles = SD.Admin)]
        public IActionResult Index()
        {
            var reviews = reviewRepository.GetAll([e => e.User, e => e.Product]);
            return View(reviews);
        }

        public IActionResult Create(int productId, string userId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You must log in to add reviews on the product.";
                return Redirect("/Identity/Account/Login");
            }
            var product = productRepository.GetOne(expression: e => e.Id == productId);
            if (product == null)
            {
                return RedirectToAction("NotFoundPage", "Home");
            }

            ViewBag.ProductId = productId;
            ViewBag.UserId = userId;

            var review = new Review
            {
                ProductId = productId,
                UserId = userId
            };

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Review review)
        {
            ViewBag.UserId = userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                reviewRepository.Add(review);

                var product = productRepository.GetOne(expression: e => e.Id == review.ProductId);

                if (product != null)
                {
                    var reviews = reviewRepository.GetAll(expression: e => e.ProductId == review.ProductId);

                    if (reviews.Any())
                    {
                        var averageRating = reviews.Average(r => r.Rating);
                        product.Rating = averageRating;
                        productRepository.Update(product);
                    }
                }

                TempData["SuccessMessage"] = "Review added successfully!";
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "There was an error adding your review.";
            return View(review);
        }
        [Authorize(Roles = SD.Admin)]
        public IActionResult Edit(int id)
        {
            var Category = reviewRepository.GetOne([], e => e.Id == id);
            if (Category == null)
            {
                return RedirectToAction("NotFoundPage", "Home");
            }
            return View(Category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Review review)
        {
            if (ModelState.IsValid)
            {
                reviewRepository.Update(review);
                TempData["SuccessMessage"] = "Review updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "There was an error updating your review.";
            return View(review);
        }

        public IActionResult Delete(int id)
        {
            Review review = new Review()
            {
                Id = id
            };
            reviewRepository.Delete(review);
            TempData["SuccessMessage"] = "Review deleted successfully!";
            return RedirectToAction("index");
        }
    }
}
