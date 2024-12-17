using DataAccess.Repository;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Rotativa.AspNetCore;
using Utility;

namespace Test.Controllers
{
    [Authorize(Roles = SD.Admin)]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICartRepository cartRepository;
        private readonly IOrderItemRepository orderItem;
        private readonly IDiscountRepository discountRepository;

        public OrderController(
            IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager,
            ICartRepository cartRepository,
            IOrderItemRepository orderItem
        )
        {
            _orderRepository = orderRepository;
            this.userManager = userManager;
            this.cartRepository = cartRepository;
            this.orderItem = orderItem;
        }

        public IActionResult Index(string searchTerm, string status = "All", int page = 1, int pageSize = 10)
        {
            var orders = _orderRepository.GetAll([e => e.User, e => e.OrderItems, e => e.Discount, e => e.Payment]);

            if (!string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
                {
                    orders = orders.Where(o => o.Status == parsedStatus).ToList();
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid order status.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                orders = orders.Where(o =>
                    o.Id.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(o.User?.FullName) && o.User.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            var totalOrders = orders.Count();
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            var ordersPaged = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.SelectedStatus = status;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(ordersPaged);
        }

      

        public IActionResult Details(int id)
        {
            var order = _orderRepository.GetOne([e => e.User, e => e.OrderItems, e => e.Payment, e => e.Discount], expression: e => e.Id == id);
            ViewBag.OrderItems = orderItem.GetAll([e => e.Product], expression: e => e.OrderId == id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("NotFoundPage", "Home");
            }
            return View(order);
        }

        public IActionResult Create()
        {
            Order order = new Order()
            {
                UserId = userManager.GetUserId(User),
            };
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Order order)
        {
            if (ModelState.IsValid)
            {
                _orderRepository.Add(order);
                TempData["SuccessMessage"] = "Order created successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "There was an error creating the order.";
            return View(order);
        }

        public IActionResult Edit(int id)
        {
            var order = _orderRepository.GetOne([e => e.Payment], expression: e => e.Id == id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("NotFoundPage", "Home");
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                _orderRepository.Update(order);
                TempData["SuccessMessage"] = "Order updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "There was an error updating the order.";
            return View(order);
        }

        public IActionResult Delete(int id)
        {
            Order order = new Order() { Id = id };
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("NotFoundPage", "Home");
            }
            _orderRepository.Delete(order);
            TempData["SuccessMessage"] = "Order deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
