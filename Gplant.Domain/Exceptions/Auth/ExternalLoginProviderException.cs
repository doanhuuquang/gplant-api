namespace Gplant.Domain.Exceptions.Auth
{
    public class ExternalLoginProviderException(string provider, string message) : Exception($"External login with provider '{provider}' failed: {message}");
}
