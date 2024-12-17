using DataAccess.Repository;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Utility;

namespace Test.Controllers
{
    [Authorize(Roles = SD.Admin)]
    public class OrderItemController : Controller
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductRepository productRepository;
        private readonly IDiscountRepository discountRepository;

        public OrderItemController(
            IOrderItemRepository orderItemRepository , 
            IProductRepository productRepository
           )
        {
            _orderItemRepository = orderItemRepository;
            this.productRepository = productRepository;
            
        }


        public IActionResult Index()
        {
            var orderItems = _orderItemRepository.GetAll([e=> e.Order.User, e=> e.Order , e=> e.Product]);
            return View(orderItems);
        }
        public IActionResult Details(int id)
        {
            var orderItem = _orderItemRepository.GetOne(expression : e=> e.Id == id);
           
            if (orderItem == null)
            {
                return RedirectToAction("NotFoundPage", "Home"); ;
            }
            return View(orderItem);
        }
        public IActionResult Create()
        {
            OrderItem orderItem = new OrderItem();
            ViewBag.Product = productRepository.GetAll();
            return View(orderItem);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Create(OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
               
                _orderItemRepository.Add(orderItem);
                return RedirectToAction(nameof(Index));
            }
            return View(orderItem);
        }

        public IActionResult Edit(int id)
        {
            var orderItem = _orderItemRepository.GetOne([e=>e.Product , e => e.Order ],expression:e=> e.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }
            return View(orderItem);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Edit(OrderItem orderItem)
        {

            if (ModelState.IsValid)
            {
                _orderItemRepository.Update(orderItem);
                return RedirectToAction("Index", "Order");
            }
            return View(orderItem);
        }

        public IActionResult Delete(int id)
        {
            OrderItem orderItem = new OrderItem() { Id = id };
            _orderItemRepository.Delete(orderItem);
            return RedirectToAction("Index" , "Order");
        }

    }
}
