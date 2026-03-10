namespace Gplant.Domain.Exceptions.Order
{
    public class InvalidOrderStatusException(string message) : Exception(message)
    {
    }
}