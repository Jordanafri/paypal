using Ecclesia.DataAccess.Repository.IRepository;
using Ecclesia.Models;
using Ecclesia.Models.ViewModels;
using Ecclesia.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using PayPal.Api;
using PayPal;

namespace EcclesiaWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PayPalClient _payPalClient;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork, PayPalClient payPalClient)
        {
            _unitOfWork = unitOfWork;
            _payPalClient = payPalClient;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            ShoppingCartVM.OrderHeader.OrderTotal = ShoppingCartVM.ShoppingCartList.Sum(cart => cart.Product.ListPrice);
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product") ?? new List<ShoppingCart>(),
                OrderHeader = new OrderHeader()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            if (ShoppingCartVM.OrderHeader.ApplicationUser != null)
            {
                ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
                ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
                ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
                ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
                ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
                ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            }

            ShoppingCartVM.OrderHeader.OrderTotal = ShoppingCartVM.ShoppingCartList.Sum(cart => cart.Product.ListPrice);
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ShoppingCartVM.OrderHeader.OrderTotal = ShoppingCartVM.ShoppingCartList.Sum(cart => cart.Product.ListPrice);

            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            try
            {
                // PayPal integration
                var payer = new Payer() { payment_method = "paypal" };

                var redirectUrls = new RedirectUrls()
                {
                    cancel_url = "https://localhost:7288/customer/cart/index",
                    return_url = "https://localhost:7288/customer/cart/OrderConfirmation?id=" + ShoppingCartVM.OrderHeader.Id
                };

                var itemList = new ItemList() { items = new List<Item>() };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    itemList.items.Add(new Item()
                    {
                        name = item.Product.Title,
                        currency = "USD",
                        price = item.Product.ListPrice.ToString("F2", CultureInfo.InvariantCulture),
                        quantity = "1",
                        sku = "sku"
                    });
                }

                var details = new Details()
                {
                    tax = "0.00",
                    shipping = "0.00",
                    subtotal = ShoppingCartVM.OrderHeader.OrderTotal.ToString("F2", CultureInfo.InvariantCulture)
                };

                var amount = new Amount()
                {
                    currency = "USD",
                    total = (decimal.Parse(details.subtotal, CultureInfo.InvariantCulture) +
                             decimal.Parse(details.tax, CultureInfo.InvariantCulture) +
                             decimal.Parse(details.shipping, CultureInfo.InvariantCulture)).ToString("F2", CultureInfo.InvariantCulture),
                    details = details
                };

                var transactionList = new List<Transaction>
        {
            new Transaction()
            {
                description = "Transaction description",
                invoice_number = Guid.NewGuid().ToString(),
                amount = amount,
                item_list = itemList
            }
        };

                var payment = new Payment()
                {
                    intent = "sale",
                    payer = payer,
                    transactions = transactionList,
                    redirect_urls = redirectUrls
                };

                // Synchronous payment creation
                var apiContext = new APIContext(new OAuthTokenCredential(SD.PayPalClientId, SD.PayPalClientSecret).GetAccessToken())
                {
                    Config = new Dictionary<string, string>
            {
                { "mode", "sandbox" } // Or "live" for production
            }
                };
                var createdPayment = payment.Create(apiContext);

                var approvalUrl = createdPayment.links.FirstOrDefault(link => link.rel == "approval_url")?.href;
                if (string.IsNullOrEmpty(approvalUrl))
                {
                    throw new Exception("Could not find PayPal approval URL.");
                }

                // Update the OrderHeader with the PayPal payment ID
                _unitOfWork.OrderHeader.UpdatePaypalPaymentID(ShoppingCartVM.OrderHeader.Id, createdPayment.id, null);
                _unitOfWork.Save();

                return Redirect(approvalUrl);
            }
            catch (PayPalException ex)
            {
                // Log the error and provide feedback
                TempData["ErrorMessage"] = $"An error occurred during the PayPal payment process: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }



        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");

            var paymentId = Request.Query["paymentId"].ToString();
            var payerId = Request.Query["PayerID"].ToString();

            var executePaymentData = new
            {
                payer_id = payerId
            };

            try
            {
                var response = await _payPalClient.MakeAuthorizedRequestAsync(HttpMethod.Post, $"https://api.sandbox.paypal.com/v1/payments/payment/{paymentId}/execute", new StringContent(JsonSerializer.Serialize(executePaymentData), Encoding.UTF8, "application/json"));
                var responseContent = await response.Content.ReadAsStringAsync();

                var executedPayment = JsonSerializer.Deserialize<PayPalPaymentResponse>(responseContent);

                if (executedPayment.state.ToLower() != "approved")
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                    orderHeader.OrderStatus = SD.StatusCancelled;
                }
                else
                {
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                }

                _unitOfWork.Save();

                HttpContext.Session.Clear();
                return View(id);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred during the PayPal payment confirmation process: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked: true);
            if (cartFromDb == null)
            {
                TempData["ErrorMessage"] = "Item not found in the cart.";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();

            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count());
            TempData["SuccessMessage"] = "Item removed successfully.";

            return RedirectToAction(nameof(Index));
        }
    }

    public class PayPalPaymentResponse
    {
        public string id { get; set; }
        public string state { get; set; }
        public List<PayPalLinkDescription> links { get; set; }
    }

    public class PayPalLinkDescription
    {
        public string href { get; set; }
        public string rel { get; set; }
        public string method { get; set; }
    }
}
