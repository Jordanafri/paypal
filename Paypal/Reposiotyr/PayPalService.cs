using Ecclesia.DataAccess.Repository.IRepository;
using Ecclesia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecclesia.DataAccess.Repository
{
    public class PayPalService : IPayPalService
    {
        private readonly PayPalConfig _paypalConfig;

        public PayPalService(PayPalConfig paypalConfig)
        {
            _paypalConfig = paypalConfig;
        }

        public async Task<PayPalOrder> CreateOrder(decimal amount, PayPalConfig paypalConfig)
        {
            // TO DO: Implement PayPal order creation logic
            throw new NotImplementedException();
        }

        public async Task<PayPalResponse> CaptureOrder(string orderId, PayPalConfig paypalConfig)
        {
            // TO DO: Implement PayPal order capture logic
            throw new NotImplementedException();
        }
    }
}