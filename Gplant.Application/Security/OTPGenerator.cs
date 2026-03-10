using Gplant.Application.Interfaces;
using System.Security.Cryptography;

namespace Gplant.Application.Security
{
    public class OTPGenerator : IOTPGenerator
    {
        public string Generate(int length = 6)
        {
            if (length <= 0)
                throw new ArgumentException("OTP length must be greater than 0");

            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);

            var chars = bytes
                        .Select(b => (b % 10).ToString())
                        .ToArray();

            return string.Concat(chars);
        }
    }
}
