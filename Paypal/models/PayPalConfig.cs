using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecclesia.Models
{
    public class PayPalConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SandboxUrl { get; set; }
        public string ProductionUrl { get; set; }
    }
}
