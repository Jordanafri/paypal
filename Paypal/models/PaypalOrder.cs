﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecclesia.Models
{
    public class PayPalOrder
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}
