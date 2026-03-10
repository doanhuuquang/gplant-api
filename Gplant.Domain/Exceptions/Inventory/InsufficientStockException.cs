namespace Gplant.Domain.Exceptions.Inventory
{
    public class InsufficientStockException(string message) : Exception(message);
}