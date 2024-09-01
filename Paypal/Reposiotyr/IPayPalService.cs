using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecclesia.Models;
using PayPal.Api;
using PayPalCheckoutSdk.Orders;

namespace Ecclesia.DataAccess.Repository.IRepository
{
    public interface IPayPalService
    {
        Task<PayPalOrder> CreateOrder(decimal amount, PayPalConfig paypalConfig);
        Task<PayPalResponse> CaptureOrder(string orderId, PayPalConfig paypalConfig);
    }

    
}

