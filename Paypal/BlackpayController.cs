using Ecclesia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayPal.Api;
using PayPalCheckoutSdk.Orders;

namespace EcclesiaWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class BlackpayController : Controller
    {
        private readonly PayPalConfig _paypalConfig;
        private readonly IPayPalService _payPalService;

        public BlackpayController(PayPalConfig paypalConfig, IPayPalService payPalService)
        {
            _paypalConfig = paypalConfig;
            _payPalService = payPalService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] decimal amount)
        {
            var order = await _payPalService.CreateOrder(amount, _paypalConfig);
            return Json(new { id = order.Id });
        }

        [HttpPost]
        public async Task<IActionResult> CaptureOrder([FromBody] string orderId)
        {
            var response = await _payPalService.CaptureOrder(orderId, _paypalConfig);
            return Json(response);
        }
    }
}