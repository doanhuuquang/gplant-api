namespace Gplant.Domain.Exceptions
{
    public class UserNotExistsException(string email) : Exception($"The user with email address: {email} does not exist");
}
