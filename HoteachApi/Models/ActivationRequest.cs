using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoteachApi.Models
{
    public class ActivationRequest
    {
        public string? GoogleId { get; set; }
        public string? PaymentIntentId { get; set; }
    }

}
