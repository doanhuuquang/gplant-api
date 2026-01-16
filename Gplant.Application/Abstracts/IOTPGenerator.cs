namespace Gplant.Application.Abstracts
{
    public interface IOTPGenerator
    {
        public string Generate(int length = 6);
    }
}
