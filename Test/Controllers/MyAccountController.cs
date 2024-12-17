using DataAccess.Repository;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Test.Controllers
{
    public class MyAccountController : Controller
    {
        private readonly IOrderRepository orderRepository;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IOrderItemRepository orderItem;
        private readonly IReviewRepository reviewRepository;
        private readonly IProductRepository productRepository;
        private readonly IDiscountRepository discountRepository;

        public MyAccountController(IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager,
            IOrderItemRepository orderItem,
            IReviewRepository reviewRepository,
            IProductRepository productRepository,
            IDiscountRepository discountRepository
            )
        {
            this.orderRepository = orderRepository;
            this.userManager = userManager;
            this.orderItem = orderItem;
            this.reviewRepository = reviewRepository;
            this.productRepository = productRepository;
            this.discountRepository = discountRepository;
        }
        
        public IActionResult Index()
        {
            var userId = userManager.GetUserId(User);
            var orders = orderRepository.GetAll(
                [e => e.User,
                e => e.OrderItems,
                e => e.Payment,
                e => e.Discount],
                expression: e => e.UserId == userId);
            return View(orders);

        }
        public IActionResult Details(int id)
        {
            var userId = userManager.GetUserId(User);
            var order = orderRepository.GetOne(
                [e => e.User,
                e => e.OrderItems,
                e => e.Payment,
                e => e.Discount],
                expression: e => e.Id == id && e.UserId == userId);
            ViewBag.OrderItems = orderItem.GetAll([e => e.Product], expression: e => e.OrderId == id);
            if (order == null)
            {
                return RedirectToAction("NotFoundPage", "Home");
            }
            return View(order);
        }
        public IActionResult MyReviews()
        {
            var userId = userManager.GetUserId(User);
            var reviews = reviewRepository.GetAll(
                [e=> e.Product],
                expression: e => e.UserId == userId);
            return View(reviews);

        }
        public IActionResult Delete(int id)
        {
            Review review = new Review()
            {
                Id = id
            };
            reviewRepository.Delete(review);
            TempData["SuccessMessage"] = "Review deleted successfully!";
            return RedirectToAction("MyReviews");
        }
        public IActionResult BestSellerProducts()
        {
            var products = productRepository.GetAll();
           


            var bestSellers = products
                .OrderByDescending(p => p.SalesCount)
                .Where(p => p.SalesCount >= 2)
                .Take(5)
                .ToList();

            foreach (var item in bestSellers)
            {
                item.IsBestSeller = true;
            }


            return View(bestSellers);
        }

        public IActionResult AvailableCouponCode()
        {
            var Coupons = discountRepository.GetAll(expression: e=> e.StartDate < DateTime.Now && e.EndDate > DateTime.Now &&e.CouponCounter != 0);
            return View(Coupons);
        }
    }
}
