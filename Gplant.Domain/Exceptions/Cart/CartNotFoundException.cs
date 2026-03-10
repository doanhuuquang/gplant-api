namespace Gplant.Domain.Exceptions.Cart
{
    public class CartNotFoundException : Exception
    {
        public CartNotFoundException(string message) : base(message) { }
    }
}