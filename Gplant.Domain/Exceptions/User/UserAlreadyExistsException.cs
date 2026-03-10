using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gplant.Domain.Exceptions.User
{
    public class UserAlreadyExistsException(string email) : Exception($"User with email: {email} already exitst");
}
