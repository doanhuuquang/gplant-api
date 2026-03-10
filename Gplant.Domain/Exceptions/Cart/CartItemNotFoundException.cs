namespace Gplant.Domain.Exceptions.Cart
{
    public class CartItemNotFoundException : Exception
    {
        public CartItemNotFoundException(string message) : base(message) { }
    }
}