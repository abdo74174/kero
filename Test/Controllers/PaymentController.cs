using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Utility;

namespace Test.Controllers
{
    [Authorize(Roles = SD.Admin)]
    public class PaymentController : Controller
    {
        private readonly IPaymentRepository paymentRepository;

        public PaymentController(IPaymentRepository paymentRepository)
        {
            this.paymentRepository = paymentRepository;
        }

        public IActionResult Index(string search, string status = "All", int page = 1, int pageSize = 10)
        {
            var payments = paymentRepository.GetAll([e => e.Order, e => e.Order.User]);
            if (!string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
                {
                    payments = payments.Where(o => o.Status == parsedStatus).ToList();
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid payment status provided.";
                    return RedirectToAction("Index");

                }
            }
            if (!string.IsNullOrEmpty(search))
            {
                payments = payments.Where(p => p.Order.Id.ToString().Contains(search) ||
                                               (p.Order.User != null && p.Order.User.FullName.ToLower().Contains(search, StringComparison.OrdinalIgnoreCase)));
            }
            ViewBag.SelectedStatus = status;
            var allPayments = payments.Count();
            var totalPages = (int)Math.Ceiling((double)allPayments / pageSize);
            var paymentPaged = payments.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.SelectedStatus = status;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            return View("Index", paymentPaged);
        }

        public IActionResult PaymentDetails(int Id)
        {
            var payments = paymentRepository.GetOne([e => e.Order, e => e.Order.User, e => e.Order.OrderItems], e => e.OrderId == Id);
            if (payments == null)
            {
                return RedirectToAction("NotFoundPage", "Home");
            }
            return View(payments);
        }

        [HttpGet]
        public IActionResult Create()
        {
            Payment payment = new Payment();
            ViewBag.PaymentStatus = Enum.GetValues(typeof(PaymentStatus))
            .Cast<PaymentStatus>()
            .ToList();
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                paymentRepository.Add(payment);
                TempData["SuccessMessage"] = "Payment created successfully!";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "An error occurred while creating the payment.";
            return View(payment);
        }

        public IActionResult Edit(int Id)
        {
            var Payment = paymentRepository.GetOne([e => e.Order], e => e.OrderId == Id);
            return View(Payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Payment payment)
        {
            if (ModelState.IsValid)
            {
                paymentRepository.Update(payment);
                TempData["SuccessMessage"] = "Payment updated successfully!";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "An error occurred while updating the payment.";
            return View(payment);
        }

        public IActionResult Delete(int Id)
        {
            Payment payment = new Payment { Id = Id };
            paymentRepository.Delete(payment);
            TempData["SuccessMessage"] = "Payment deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
