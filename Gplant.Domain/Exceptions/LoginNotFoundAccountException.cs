namespace Gplant.Domain.Exceptions
{
    public class LoginNotFoundAccountException(string email) : Exception($"Not found account with email {email}");
}
