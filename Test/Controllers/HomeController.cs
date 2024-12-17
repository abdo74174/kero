using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Models;
using DataAccess.Repository.IRepository;
using DataAccess.Repository;
using Microsoft.AspNetCore.Authorization;
using Utility;

namespace Test.Controllers
{


    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository productRepository;

        public HomeController(ILogger<HomeController> logger , IProductRepository productRepository )
        {
            _logger = logger;
            this.productRepository = productRepository;
        }

        public IActionResult Index(int page = 1, int pageSize = 6)
        {

            var Products = productRepository.GetAll();


            var bestSellers = Products
                .OrderByDescending(p => p.SalesCount)
                .Where(p => p.SalesCount >= 2)
                .Take(5)
                .ToList();

            foreach (var item in bestSellers)
            {
                item.IsBestSeller = true;
            }


            var totalProducts = Products.Count();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);


            var ProductPaged = Products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(ProductPaged);
        }

        public IActionResult Search(string query, double? minPrice, double? maxPrice, int? minRate, int? maxRate)
        {
            var products = productRepository.GetAll(
                [e => e.Category],
                    e =>
                    (string.IsNullOrEmpty(query) || e.Name.Contains(query) || e.Description.Contains(query) || e.Category.Name.Contains(query)) &&
                    (!minPrice.HasValue || e.Price >= minPrice.Value) &&
                    (!maxPrice.HasValue || e.Price <= maxPrice.Value) &&
                    (!minRate.HasValue || e.Rating >= minRate.Value) &&
                    (!maxRate.HasValue || e.Rating <= maxRate.Value)
            );

            ViewData["SearchQuery"] = query;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;
            ViewData["MinRate"] = minRate;
            ViewData["MaxRate"] = maxRate;

            return View(products);
        }
        [HttpGet]
        public IActionResult Compare(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return RedirectToAction("NofFoundPage", "Home");
            }

            var productIds = ids.Split(',').Select(id => int.Parse(id)).ToList();

            var productsToCompare = productRepository.GetAll(expression: e => productIds.Contains(e.Id));
            var bestSellers = productsToCompare
               .OrderByDescending(p => p.SalesCount)
               .Where(p => p.SalesCount >= 2)
               .Take(5)
               .ToList();

            foreach (var item in bestSellers)
            {
                item.IsBestSeller = true;
            }

            return View(productsToCompare);
        }




        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult NotFoundPage(int id)
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
