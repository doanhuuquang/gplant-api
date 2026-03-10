using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gplant.Domain.Exceptions.Auth
{
    public class LoginIncorrectPasswordException() : Exception("Incorrect Password");
}
