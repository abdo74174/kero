using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModel;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using Stripe.Issuing;
using Utility;
using Discount = Models.Discount;
using Order = Models.Order;

namespace Test.Controllers
{
    public class CartController : Controller
    {

        private readonly ICartRepository cartRepository;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IOrderRepository orderRepository;
        private readonly IPaymentRepository paymentRepository;
        private readonly IProductRepository productRepository;
        private readonly IDiscountRepository discountRepository;
        private readonly IEmailSender emailSender;

        public CartController(
            ICartRepository cartRepository, UserManager<ApplicationUser> userManager,
            IOrderRepository orderRepository, IPaymentRepository paymentRepository,
            IProductRepository productRepository, IDiscountRepository discountRepository,
            IEmailSender emailSender
            )
        {
            this.cartRepository = cartRepository;
            this.userManager = userManager;
            this.orderRepository = orderRepository;
            this.paymentRepository = paymentRepository;
            this.productRepository = productRepository;
            this.discountRepository = discountRepository;
            this.emailSender = emailSender;
        }
       
        public IActionResult AddToCart(int Count, int ProductId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You must log in to add items to the cart.";
                return Redirect("/Identity/Account/Login");
            }
            Cart cart = new Cart()
            {
                Count = Count,
                ProductId = ProductId,
                UserId = userManager.GetUserId(User)
            };

            var Product = productRepository.GetOne(expression: e => e.Id == ProductId);

            if (cart != null)
            {
                cartRepository.Add(cart);
                TempData["SuccessMessage"] = "product has been added to your cart.";
                return RedirectToAction("Index", "Home");
            }
            TempData["ErrorMessage"] = "Product not found.";
            return RedirectToAction("NotFoundPage", "Home");
        }

        public IActionResult Index(string couponCode)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You must log in to add items to the cart.";
                return Redirect("/Identity/Account/Login");
            }
            var ApplicationUserId = userManager.GetUserId(User);
            var addedToCartItems = cartRepository.GetAll([e => e.Product], m => m.UserId == ApplicationUserId, false);

            var discount = discountRepository.GetOne(expression:e => e.Name == couponCode);

            double total = addedToCartItems.Sum(cart => cart.Count * cart.Product.Price);
            ViewBag.Total = total.ToString("C");

            double discountRate = 0;
            if (!string.IsNullOrEmpty(couponCode) && discount != null)
            {
                if (discount.StartDate < DateTime.Now && DateTime.Now < discount.EndDate)
                {
                    if (discount.CouponCounter > 0)
                    {
                        discountRate = discount.DiscountRate;
                    }
                    else
                    {
                        discountRate = 0;
                    }
                }
            }

            
            TempData["DiscountRate"] = discountRate.ToString("0.##"); 

            ViewBag.DiscountRate = discountRate;

            double discountAmount = (total * discountRate) / 100;
            double discountedTotal = total - discountAmount;

            ViewBag.DiscountedTotal = discountedTotal.ToString("c");
            ViewBag.DiscountMessage = discountRate > 0 ? $"Coupon applied: {discountRate}% off!" : "No coupon applied.";
            ViewBag.CouponCode = couponCode;

            return View(addedToCartItems);
        }

        public IActionResult Increment(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You must log in to add items to the cart.";
                return Redirect("/Identity/Account/Login");
            }
            var ApplicationUserId = userManager.GetUserId(User);
            var item = cartRepository.GetOne([], m => m.UserId == ApplicationUserId && m.ProductId == id);

            if (item != null)
            {
                item.Count++;
                TempData["SuccessMessage"] = "Item quantity updated.";
                cartRepository.Commit();
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Item not found in the cart.";
            return RedirectToAction("NotFoundPage", "Home");
        }

        public IActionResult Decrement(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You must log in to add items to the cart.";
                return Redirect("/Identity/Account/Login");
            }
            var ApplicationUserId = userManager.GetUserId(User);
            var item = cartRepository.GetOne([], m => m.UserId == ApplicationUserId && m.ProductId == id);

            if (item != null)
            {
                item.Count--;
                if (item.Count > 0)
                {
                    TempData["SuccessMessage"] = "Item quantity updated.";
                    cartRepository.Commit();
                }
                else
                {
                    item.Count = 1;
                    TempData["InfoMessage"] = "Minimum quantity is 1.";
                }

                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Item not found in the cart.";
            return RedirectToAction("NotFoundPage", "Home");
        }

        public IActionResult Delete(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You must log in to add items to the cart.";
                return Redirect("/Identity/Account/Login");
            }
            var ApplicationUserId = userManager.GetUserId(User);
            var item = cartRepository.GetOne([], m => m.UserId == ApplicationUserId && m.ProductId == id);

            if (item != null)
            {
                cartRepository.Delete(item);
                TempData["SuccessMessage"] = "Item removed from the cart.";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Item not found in the cart.";
            return RedirectToAction("NotFoundPage", "Home");
        }

        public IActionResult Pay(string couponCode)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You must log in to add items to the cart.";
                return Redirect("/Identity/Account/Login");
            }
            var stripeSecretKey = "sk_test_51QSg3CAQAE6DGCi2yxMSE33w9PymFc6hk1IAzrFcxAUBR8IMyUDeoMmlXUgcAjxMHQuAurdB201q8s5qipUViStE00OYBusMf5"; // Replace with your actual secret key
            StripeConfiguration.ApiKey = stripeSecretKey;

            var ApplicationUserId = userManager.GetUserId(User);
            var addedToCartItems = cartRepository.GetAll([e => e.Product], m => m.UserId == ApplicationUserId);

            if (!addedToCartItems.Any())
            {
                TempData["ErrorMessage"] = "No items in your cart to proceed with payment.";
                return RedirectToAction("CheckoutCancel", "Cart", new { message = "No items in your cart." });
            }

            var discount = discountRepository.GetOne(expression:e => e.Name == couponCode);
            double discountRate = 0;
            if (!string.IsNullOrEmpty(couponCode) && discount != null)
            {
                if (discount.StartDate < DateTime.Now && DateTime.Now < discount.EndDate)
                {
                    if (discount.CouponCounter > 0)
                    {
                        discountRate = discount.DiscountRate;
                    }
                    else
                    {
                        discountRate = 0;
                    }
                }
            }

           
            TempData["DiscountRate"] = discountRate.ToString("0.##"); 

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Cart/CheckoutSuccess?couponCode={couponCode}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Cart/CheckoutCancel",
            };

            foreach (var item in addedToCartItems)
            {
                double originalPrice = item.Product.Price;
                double discountedPrice = originalPrice - (originalPrice * discountRate / 100);

                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                            Description = item.Product.Description,
                        },
                        UnitAmount = (long)(discountedPrice * 100),
                    },
                    Quantity = item.Count,
                });
            }

            var service = new SessionService();
            var session = service.Create(options);
          
            return Redirect(session.Url);
        }



        public async Task<IActionResult> CheckoutSuccess(string couponCode)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You must log in to add items to the cart.";
                return Redirect("/Identity/Account/Login");
            }
            var ApplicationUserId = userManager.GetUserId(User);
            var UserFullName = userManager.GetUserAsync(User).Result.FullName;
            var UserEmail = userManager.GetUserAsync(User).Result.Email;
            var addedToCartItems = cartRepository.GetAll([e => e.Product], m => m.UserId == ApplicationUserId);

            if (!addedToCartItems.Any())
            {
                return RedirectToAction("Index", "Home");
            }
            var discount = discountRepository.GetOne(expression: e => e.Name == couponCode);
            double discountRate = 0;
            if (!string.IsNullOrEmpty(couponCode) && discount != null)
            {
                if (discount.StartDate < DateTime.Now && DateTime.Now < discount.EndDate)
                {
                    if (discount.CouponCounter > 0)
                    {
                        discountRate = discount.DiscountRate;
                        discount.CouponCounter -= 1;
                        discountRepository.Update(discount);
                    }
                    else
                    {
                        discountRate = 0;
                    }
                }
            }
            double totalAmountBeforeDiscount = addedToCartItems.Sum(item => item.Count * item.Product.Price);

            double discountAmount = totalAmountBeforeDiscount * (discountRate / 100);
            double totalAmountAfterDiscount = totalAmountBeforeDiscount - discountAmount;

            var order = new Order
            {
                UserId = ApplicationUserId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmountAfterDiscount,
                OrderItems = addedToCartItems.Select(cartItem => new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Count,
                    UnitPrice = cartItem.Product.Price,
                }).ToList()
            };
            string adminMessage = $"new order is received by {UserFullName}, need to be checked";
            string userMessage = $"Hello , Mr :{UserFullName}, thank you for making the order " +
                $", the order will be received during week from know , " +
                $"for more details go to web site / my orders ";
            await emailSender.SendEmailAsync(SD.AdminEmail, "New Order ", adminMessage);
            await emailSender.SendEmailAsync(UserEmail, "New Order ", userMessage);
            orderRepository.Add(order);
            TempData["OrderId"] = order.Id;
           
            
            foreach (var item in addedToCartItems)
            {
                var product = productRepository.GetOne(expression: e => e.Id == item.ProductId);

                if (product != null)
                {
                    product.Stock -= item.Count;
                    product.SalesCount += item.Count;
                    if (product.Stock == 0)
                    {
                        var message = $"these products : $/$ {product.Name} $/$ is out of stock , please ship order";
                        await emailSender.SendEmailAsync(SD.AdminEmail, "Out Of Stock Products", message);
                    }
                    productRepository.Update(product);
                }
            }

            foreach (var item in addedToCartItems)
            {
                cartRepository.Delete(item);
            }

            return View();
        }
        public async Task<IActionResult> CashSuccess(string couponCode)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You must log in to add items to the cart.";
                return Redirect("/Identity/Account/Login");
            }
            var ApplicationUserId = userManager.GetUserId(User);
            var addedToCartItems = cartRepository.GetAll([e => e.Product], m => m.UserId == ApplicationUserId);

            if (!addedToCartItems.Any())
            {
                return RedirectToAction("Index", "Home");
            }
            var discount = discountRepository.GetOne(expression: e => e.Name == couponCode);
            double discountRate = 0;
            if (!string.IsNullOrEmpty(couponCode) && discount != null)
            {
                if (discount.StartDate < DateTime.Now && DateTime.Now < discount.EndDate)
                {
                    if (discount.CouponCounter > 0)
                    {
                        discountRate = discount.DiscountRate;
                        discount.CouponCounter -= 1;
                        discountRepository.Update(discount);
                    }
                    else
                    {
                        discountRate = 0;
                    }
                }
            }

            double totalAmountBeforeDiscount = addedToCartItems.Sum(item => item.Count * item.Product.Price);
            double discountAmount = totalAmountBeforeDiscount * (discountRate / 100);
            double totalAmountAfterDiscount = totalAmountBeforeDiscount - discountAmount;

            var order = new Order
            {
                UserId = ApplicationUserId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmountAfterDiscount,
                OrderItems = addedToCartItems.Select(cartItem => new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Count,
                    UnitPrice = cartItem.Product.Price,
                }).ToList()
            };
            var UserFullName = userManager.GetUserAsync(User).Result.FullName;
            var userEmail = userManager.GetUserAsync(User).Result.Email;

            string adminMessage = $"new order is received by {UserFullName}, need to be reviewed";
            string userMessage = $"Hello , Mr :{UserFullName}, thank you for making the order " +
                $", the order will be received during week from know , " +
                $"for more details go to web site / my orders ";
            await emailSender.SendEmailAsync(SD.AdminEmail, "New Order ", adminMessage);
            await emailSender.SendEmailAsync(userEmail, "New Order ", userMessage);

            orderRepository.Add(order);
            TempData["OrderId"] = order.Id;

            foreach (var item in addedToCartItems)
            {
                var product = productRepository.GetOne(expression: e => e.Id == item.ProductId);
                if (product != null)
                {
                    product.Stock -= item.Count;
                    product.SalesCount += item.Count;
                    if (product.Stock == 0)
                    {
                        var message = $"these products : $/$ {product.Name} $/$ is out of stock , please ship order";
                        await emailSender.SendEmailAsync(SD.AdminEmail, "Out Of Stock Products", message);
                    }
                    productRepository.Update(product);
                }
            }

            foreach (var item in addedToCartItems)
            {
                cartRepository.Delete(item);
            }

            return View();
        }
        public IActionResult SaveShippingAddress(ShippingAddtressViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You must log in to add items to the cart.";
                return Redirect("/Identity/Account/Login");
            }
            var ApplicationUserId = userManager.GetUserId(User);

            var orderId = TempData["OrderId"] as int?;

            if (orderId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = orderRepository.GetOne(expression: e => e.Id == orderId.Value);

            if (order != null)
            {
                order.ShippingAddress = $"{model.StreetAddress}, {model.City}, {model.State}, {model.PostalCode}, {model.Country}";
                TempData["SuccessMessage"] = "Shipping address saved successfully.";
                orderRepository.Update(order);
                Payment payment = new Payment()
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    Status = PaymentStatus.success,
                    PaymentMethod = "stripe",
                    PaymentDate = DateTime.Now,
                };

                paymentRepository.Add(payment);

               
                return RedirectToAction("OrderConfirmation", "Cart");
            }
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction("NotFoundPage", "Home");
        }
        public IActionResult SaveShippingAddressUser(ShippingAddtressViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You must log in to add items to the cart.";
                return Redirect("/Identity/Account/Login");
            }
            var ApplicationUserId = userManager.GetUserId(User);

            var orderId = TempData["OrderId"] as int?;

            if (orderId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = orderRepository.GetOne(expression: e => e.Id == orderId.Value);

            if (order != null)
            {
                order.ShippingAddress = $"{model.StreetAddress}, {model.City}, {model.State}, {model.PostalCode}, {model.Country}";

                orderRepository.Update(order);
                Payment payment = new Payment()
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    Status = PaymentStatus.pending,
                    PaymentMethod = "Cash",
                    PaymentDate = DateTime.Now,
                };

                paymentRepository.Add(payment);


                return RedirectToAction("OrderConfirmation", "Cart");
            }

            return RedirectToAction("NotFoundPage", "Home");
        }

        public IActionResult OrderConfirmation()
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You must log in to add items to the cart.";
                return Redirect("/Identity/Account/Login");
            }
            return View();
        }

        public IActionResult CheckoutCancel()
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You must log in to add items to the cart.";
                return Redirect("/Identity/Account/Login");
            }
            TempData["ErrorMessage"] = "Payment was canceled.";
            return View();
        }
    }
}
