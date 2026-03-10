namespace Gplant.Domain.Exceptions.Auth
{
    public class LoginNotFoundAccountException(string email) : Exception($"Not found account with email {email}");
}
