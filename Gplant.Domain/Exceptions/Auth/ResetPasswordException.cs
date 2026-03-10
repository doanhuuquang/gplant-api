namespace Gplant.Domain.Exceptions.Auth
{
    public class ResetPasswordException(IEnumerable<string> errorDescriptions) : Exception($"Reset password failed with following errors: ${string.Join(Environment.NewLine, errorDescriptions)}");
}
