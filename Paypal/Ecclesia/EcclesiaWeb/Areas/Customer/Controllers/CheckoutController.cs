//using Microsoft.AspNetCore.Mvc;
//using System.Text.Json.Nodes;
//using System.Text;
//using Microsoft.AspNetCore.Authorization;

//namespace EcclesiaWeb.Areas.Customer.Controllers
//{
//    [Area("Customer")]
//    [Authorize]
//    public class CheckoutController : Controller
//    {
//        private string PaypalClientId { get; set; } = "";
//        private string PaypalSecret { get; set; } = "";
//        private string PaypalUrl { get; set; } = "";

//        public CheckoutController(IConfiguration configuration)
//        {
//            PaypalClientId = configuration["PayPal:ClientId"]!;
//            PaypalSecret = configuration["PayPal:Secret"]!;
//            PaypalUrl = configuration["PayPal:Url"]!;
//        }

//        public IActionResult Index()
//        {
//            ViewBag.PaypalClientId = PaypalClientId;
//            return View();
//        }


//        [HttpPost]
//        public async Task<JsonResult> CreateOrder([FromBody] JsonObject data)
//        {
//            var totalAmount = data?["amount"]?.ToString();
//            if (totalAmount == null)
//            {
//                return new JsonResult(new { Id = "" });
//            }

//            //create the request body
//            JsonObject createOrderRequest = new JsonObject();
//            createOrderRequest.Add("intent", "CAPTURE");

//            JsonObject amount = new JsonObject();
//            amount.Add("currency_code", "ZAR");
//            amount.Add("value", totalAmount);

//            JsonObject purchaseUnit1 = new JsonObject();
//            purchaseUnit1.Add("amount", amount);

//            JsonArray purchaseUnits = new JsonArray();
//            purchaseUnits.Add(purchaseUnit1);

//            createOrderRequest.Add("purchase_units", purchaseUnits);

//            // get access token
//            string accessToken = await GetPaypalAccessToken();

//            //send request
//            string url = PaypalUrl + "/v2/checkout/orders";

//            //create http client
//            using (var client = new HttpClient())
//            {
//                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

//                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
//                requestMessage.Content = new StringContent(createOrderRequest.ToString(), null, "application/json");

//                var httpResponse = await client.SendAsync(requestMessage);

//                if (httpResponse.IsSuccessStatusCode)
//                {
//                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
//                    var jsonResponse = JsonNode.Parse(strResponse);

//                    if (jsonResponse != null)
//                    {
//                        string paypalOrderId = jsonResponse["id"]?.ToString() ?? "";

//                        // return a valid PayPal order object
//                        return new JsonResult(new { Id = paypalOrderId });
//                    }
//                }
//            }


//            return new JsonResult(new { Id = "" });
//        }


//        [HttpPost]
//        public async Task<JsonResult> CompleteOrder([FromBody] JsonObject data)
//        {
//            var orderId = data?["orderID"]?.ToString();
//            if (orderId == null)
//            {
//                return new JsonResult("error");

//            }

//            //get access token
//            string accessToken = await GetPaypalAccessToken();


//            //send request 22:20
//            string url = PaypalUrl + "/v2/checkout/orders/" + orderId + "/capture";

//            //http client
//            using (var client = new HttpClient())
//            {
//                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

//                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
//                requestMessage.Content = new StringContent("", null, "application/json");

//                var httpResponse = await client.SendAsync(requestMessage);

//                //recieve response
//                if (httpResponse.IsSuccessStatusCode)
//                {
//                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
//                    var jsonResponse = JsonNode.Parse(strResponse);

//                    if (jsonResponse != null)
//                    {
//                        string paypalOrderStatus = jsonResponse["status"]?.ToString() ?? "";
//                        if (paypalOrderStatus == "COMPLETED")
//                        {
//                            // save the order in the database

//                            return new JsonResult("success");
//                        }
//                    }
//                }
//            }

//            return new JsonResult("error");
//        }


//        private async Task<string> GetPaypalAccessToken()
//        {

//            string accessToken = "";
//            string url = PaypalUrl + "/v1/oauth2/token";

//            using (var client = new HttpClient())
//            {
//                string credentials64 =
//                    Convert.ToBase64String(Encoding.UTF8.GetBytes(PaypalClientId + ":" + PaypalSecret));
//                client.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials64);

//                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
//                requestMessage.Content = new StringContent("grant_type=client_credentials", null
//                    , "application/x-www-form-urlencoded");

//                var httpResponse = await client.SendAsync(requestMessage);

//                if (httpResponse.IsSuccessStatusCode)
//                {
//                    var strResponse = await httpResponse.Content.ReadAsStringAsync();

//                    var jsonResponse = JsonNode.Parse(strResponse);
//                    if (jsonResponse != null)
//                    {
//                        accessToken = jsonResponse["access_token"]?.ToString() ?? "";
//                    }
//                }

//            }

//            return accessToken;
//        }






//    }
//}

using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace EcclesiaWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ILogger _logger;
        private readonly string _paypalClientId;
        private readonly string _paypalSecret;
        private readonly string _paypalUrl;

        private static string _cachedAccessToken;
        private static DateTime _accessTokenExpiration;

        public CheckoutController(ILogger<CheckoutController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _paypalClientId = configuration["PayPal:ClientId"]!;
            _paypalSecret = configuration["PayPal:Secret"]!;
            _paypalUrl = configuration["PayPal:Url"]!;
        }

        public IActionResult Index()
        {
            ViewBag.PaypalClientId = _paypalClientId;
            return View();
        }


        [HttpPost]
        public async Task<JsonResult> CreateOrder([FromBody] JsonObject data)
        {
            _logger.LogInformation("CreateOrder method started.");
            var totalAmount = data?["amount"]?.ToString();
            decimal amount;
            if (totalAmount == null || !decimal.TryParse(totalAmount, out amount))
            {
                _logger.LogError("Invalid or missing amount value.");
                return new JsonResult(new { Id = "" });
            }
            _logger.LogInformation($"Processing order with amount: {amount}.");
            //create the request body
            JsonObject createOrderRequest = new JsonObject();
            createOrderRequest.Add("intent", "CAPTURE");

            JsonObject amountObject = new JsonObject();
            amountObject.Add("currency_code", "USD");
            amountObject.Add("value", amount.ToString());

            JsonObject purchaseUnit1 = new JsonObject();
            purchaseUnit1.Add("amount", amountObject);

            JsonArray purchaseUnits = new JsonArray();
            purchaseUnits.Add(purchaseUnit1);

            createOrderRequest.Add("purchase_units", purchaseUnits);


            // get access token
            _logger.LogInformation("Attempting to get PayPal access token.");
            string accessToken = await GetPaypalAccessToken();

            //send request
            string url = _paypalUrl + "/v2/checkout/orders";

            //create http client
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent(createOrderRequest.ToString(), null, "application/json");

                _logger.LogInformation($"Sending order creation request to PayPal: {url}");
                var httpResponse = await client.SendAsync(requestMessage);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogInformation("Order created successfully. PayPal response: " + strResponse);
                    var jsonResponse = JsonNode.Parse(strResponse);
                    _logger.LogInformation($"Order creation response: {strResponse}");

                    if (jsonResponse != null)
                    {
                        string paypalOrderId = jsonResponse["id"]?.ToString() ?? "";

                        // return a valid PayPal order object
                        return new JsonResult(new { Id = paypalOrderId });
                    }
                }
                else
                {
                    _logger.LogError($"Failed to create PayPal order. Status: {httpResponse.StatusCode}, Reason: {httpResponse.ReasonPhrase}");
                }
            }


            return new JsonResult(new { Id = "" });
        }

        [HttpPost]
        public async Task<JsonResult> CompleteOrder([FromBody] JsonObject data)
        {
            _logger.LogInformation("CompleteOrder method started.");
            var orderId = data?["orderID"]?.ToString();
            if (string.IsNullOrEmpty(orderId))
            {
                _logger.LogError("OrderId is null or empty");
                return new JsonResult(new { error = "Invalid order ID" });
            }

            _logger.LogInformation($"Processing payment capture for Order ID: {orderId}.");

            try
            {
                // Get access token
                _logger.LogInformation("Attempting to get PayPal access token.");
                string accessToken = await GetPaypalAccessToken();

                // Send request
                string url = _paypalUrl + "/v2/checkout/orders/" + orderId + "/capture";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                    requestMessage.Content = new StringContent("", null, "application/json");

                    _logger.LogInformation($"Sending payment capture request to PayPal: {url}");
                    var httpResponse = await client.SendAsync(requestMessage);

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var strResponse = await httpResponse.Content.ReadAsStringAsync();
                        _logger.LogInformation("Payment captured successfully. PayPal response: " + strResponse);
                        var jsonResponse = JsonNode.Parse(strResponse);

                        if (jsonResponse != null)
                        {
                            string paypalOrderStatus = jsonResponse["status"]?.ToString() ?? "";
                            if (paypalOrderStatus == "COMPLETED")
                            {
                                // Save the order in the database
                                return new JsonResult(new { success = true });
                            }
                            else
                            {
                                return new JsonResult(new { error = "Payment failed" });
                            }
                        }
                        else
                        {
                            return new JsonResult(new { error = "Invalid payment response" });
                        }
                    }
                    else
                    {
                        _logger.LogError($"Payment capture failed: {httpResponse.StatusCode} - {httpResponse.ReasonPhrase}");
                        return new JsonResult(new { error = "Payment capture failed" });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing order");
                return new JsonResult(new { error = "An error occurred" });
            }
        }

        private async Task<string> GetPaypalAccessToken()
        {

            // Check if the cached token is still valid
            if (!string.IsNullOrEmpty(_cachedAccessToken) && DateTime.Now < _accessTokenExpiration)
            {
                _logger.LogInformation("Reusing cached PayPal access token.");
                return _cachedAccessToken;
            }

            _logger.LogInformation("Attempting to get PayPal access token.");
            string accessToken = "";
            string url = _paypalUrl + "/v1/oauth2/token";

            using (var client = new HttpClient())
            {
                string credentials = $"{_paypalClientId}:{_paypalSecret}";
                byte[] credentialsBytes = Encoding.UTF8.GetBytes(credentials);
                string credentials64 = Convert.ToBase64String(credentialsBytes);
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials64);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

                var httpResponse = await client.SendAsync(requestMessage);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();

                    var jsonResponse = JsonNode.Parse(strResponse);
                    if (jsonResponse != null)
                    {
                        accessToken = jsonResponse["access_token"]?.ToString() ?? "";
                        var expiresIn = jsonResponse["expires_in"]?.ToString();

                        if (!string.IsNullOrEmpty(accessToken) && int.TryParse(expiresIn, out int expiresInSeconds))
                        {
                            // Cache the token and set its expiration time
                            _cachedAccessToken = accessToken;
                            _accessTokenExpiration = DateTime.Now.AddSeconds(expiresInSeconds - 60); // Subtracting 60 seconds as a buffer
                        }
                    }
                }
            }

            return accessToken;
        }

    }
}