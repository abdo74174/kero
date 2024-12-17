using DataAccess.Repository;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Globalization;
using Utility;

namespace Test.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository productRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IReviewRepository reviewRepository;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IOrderRepository orderRepository;
        private readonly IEmailSender emailSender;
        private readonly IPaymentRepository paymentRepository;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IReviewRepository reviewRepository,
            UserManager<ApplicationUser> userManager,
            IOrderRepository orderRepository,
            IEmailSender emailSender,
            IPaymentRepository paymentRepository)
        {
            this.productRepository = productRepository;
            this.categoryRepository = categoryRepository;
            this.reviewRepository = reviewRepository;
            this.userManager = userManager;
            this.orderRepository = orderRepository;
            this.emailSender = emailSender;
            this.paymentRepository = paymentRepository;
        }
        [Authorize(Roles = SD.Admin)]
        public IActionResult Index(int page = 1, int pageSize = 6)
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

            var totalProducts = products.Count();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            var productPaged = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(productPaged);
        }

        public IActionResult Details(int id)
        {
            var product = productRepository.GetOne([e => e.Category], e => e.Id == id);
            string userId = userManager.GetUserId(User);
            ViewBag.UserId = userId;
            ViewBag.Reviews = reviewRepository.GetAll([e => e.User], expression: e => e.ProductId == id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found!";
                return RedirectToAction("NotFoundPage", "Home");
            }

            bool hasPurchased = false;
            if (!string.IsNullOrEmpty(userId))
            {
                hasPurchased = orderRepository.GetAll([e => e.OrderItems])
                    .Any(o => o.UserId == userId && o.OrderItems.Any(oi => oi.ProductId == id));
            }

            ViewBag.HasPurchased = hasPurchased;
            ViewBag.Stock = product.Stock;
            return View(product);
        }
        [Authorize(Roles = SD.Admin)]
        public IActionResult Search(string query)
        {
            var products = productRepository.GetAll([e => e.Category], e => e.Name.Contains(query) || e.Description.Contains(query) || e.Category.Name.Contains(query));
            return View(products);
        }
        [Authorize(Roles = SD.Admin)]
        public IActionResult Create()
        {
            Product product = new Product();
            var categories = categoryRepository.GetAll().Select(e => new SelectListItem { Text = e.Name, Value = e.Id.ToString() });
            ViewBag.Categories = categories;
            return View(product);
        }
        [Authorize(Roles = SD.Admin)]
        [HttpPost]
        public IActionResult Create(Product product, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageUrl.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        imageUrl.CopyTo(stream);
                    }
                    product.ImageUrl = fileName;
                }

                productRepository.Add(product);
                TempData["successMessage"] = "Product created successfully!";
                return RedirectToAction("Index");
            }

            var categories = categoryRepository.GetAll().Select(e => new SelectListItem { Text = e.Name, Value = e.Id.ToString() });
            ViewBag.Categories = categories;
            TempData["ErrorMessage"] = "Failed to create product. Please try again.";
            return View(product);
        }
        [Authorize(Roles = SD.Admin)]
        public IActionResult Edit(int id)
        {
            var product = productRepository.GetOne(expression: e => e.Id == id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found!";
                return RedirectToAction("NotFoundPage", "Home");
            }

            var categories = categoryRepository.GetAll();
            ViewBag.Categories = categories;
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = SD.Admin)]
        public IActionResult Edit(Product product, IFormFile imageUrl)
        {
            ModelState.Remove("ImageUrl");
            var oldProduct = productRepository.GetOne([], expression: e => e.Id == product.Id, tracked: false);
            if (oldProduct == null)
            {
                TempData["ErrorMessage"] = "Product not found!";
                return RedirectToAction("NotFoundPage", "Home");
            }

            if (ModelState.IsValid)
            {
                if (imageUrl != null && imageUrl.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageUrl.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", oldProduct.ImageUrl);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        imageUrl.CopyTo(stream);
                    }

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                    product.ImageUrl = fileName;
                }
                else
                {
                    product.ImageUrl = oldProduct.ImageUrl;
                }

                productRepository.Update(product);
                TempData["successMessage"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            var categories = categoryRepository.GetAll().Select(e => new SelectListItem { Text = e.Name, Value = e.Id.ToString() });
            ViewBag.Categories = categories;
            TempData["ErrorMessage"] = "Failed to update product. Please try again.";
            product.ImageUrl = oldProduct.ImageUrl;
            return View(product);
        }
        [Authorize(Roles = SD.Admin)]
        public IActionResult Delete(int id)
        {
            var product = productRepository.GetOne([], e => e.Id == id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found!";
                return RedirectToAction(nameof(Index));
            }

            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", product.ImageUrl);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            productRepository.Delete(product);
            TempData["successMessage"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = SD.Admin)]
        public IActionResult Statistics()
        {
            var users = userManager.Users.ToList();
            var orders = orderRepository.GetAll();
            var newestOrders = orderRepository.GetAll(expression: e => e.Status == OrderStatus.Pending);
            var salesData = orderRepository.GetAll().Where(s => s.OrderDate >= DateTime.Now.AddMonths(-6) && s.Status != OrderStatus.Canceled)
                            .GroupBy(s => s.OrderDate.Month)
                            .Select(g => new
                            {
                                Month = g.Key,
                                TotalSales = g.Sum(s => s.TotalAmount)
                            })
                            .OrderBy(s => s.Month)
                            .ToList();

            var payments = paymentRepository.GetAll(expression: e => e.Status == PaymentStatus.success);
            var salesMonths = salesData.Select(s => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(s.Month)).ToArray();
            var salesAmounts = salesData.Select(s => s.TotalSales).ToArray();

            ViewBag.TotalPayment = payments.Sum(p => p.Amount);
            ViewBag.SalesMonths = salesMonths;
            ViewBag.SalesAmounts = salesAmounts;
            ViewBag.TotalSales = orders.Where(p=> p.Status != OrderStatus.Canceled).Sum(p => p.TotalAmount);
            ViewBag.TotalOrdersNo = newestOrders.Count();
            ViewBag.Users = users.Count();

            return View();
        }
        [Authorize(Roles = SD.Admin)]
        public IActionResult OutOfStockProduct()
        {
            var products = productRepository.GetAll(expression: e => e.Stock == 0);
            return View(products);
        }
    }
}
