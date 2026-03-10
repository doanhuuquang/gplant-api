namespace Gplant.Application.Interfaces
{
    public interface IOTPGenerator
    {
        public string Generate(int length = 6);
    }
}
