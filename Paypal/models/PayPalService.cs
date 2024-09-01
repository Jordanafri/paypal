using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecclesia.DataAccess.Repository.IRepository;
namespace Ecclesia.Models
{
    public class PayPalService : IPayPalService
    {
        public async Task<PayPalOrder> CreateOrder(decimal amount, PayPalConfig paypalConfig)
        {
            var accessToken = await GetPayPalAccessToken(paypalConfig);

            var order = new PayPalOrder
            {
                Intent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnit>
            {
                new PurchaseUnit
                {
                    Amount = new Amount
                    {
                        CurrencyCode = "ZAR",
                        Value = amount.ToString("F2")
                    }
                }
            }
            };

            var response = await PayPalApi.CreateOrder(accessToken, order);
            return response;
        }

        public async Task<PayPalResponse> CaptureOrder(string orderId, PayPalConfig paypalConfig)
        {
            var accessToken = await GetPayPalAccessToken(paypalConfig);

            var response = await PayPalApi.CaptureOrder(accessToken, orderId);
            return response;
        }

        private async Task<string> GetPayPalAccessToken(PayPalConfig paypalConfig)
        {
            var clientId = paypalConfig.ClientId;
            var clientSecret = paypalConfig.ClientSecret;
            var sandboxUrl = paypalConfig.SandboxUrl;

            var tokenResponse = await PayPalApi.GetAccessToken(clientId, clientSecret, sandboxUrl);
            return tokenResponse.AccessToken;
        }
    }
}
