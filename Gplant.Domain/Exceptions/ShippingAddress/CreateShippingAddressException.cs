using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gplant.Domain.Exceptions.ShippingAddress
{
    public class CreateShippingAddressException(string message) : Exception(message);
}
