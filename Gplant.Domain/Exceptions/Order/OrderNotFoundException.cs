namespace Gplant.Domain.Exceptions.Order
{
    public class OrderNotFoundException(string message) : Exception(message)
    {
    }
}