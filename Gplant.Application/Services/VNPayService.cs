using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Gplant.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Gplant.Application.Services
{
    public class VNPayService(IConfiguration config) : IVNPayService
    {
        private readonly IConfigurationSection _cfg = config.GetSection("VNPay");

        private static readonly TimeZoneInfo _vnTz =
            TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");

        public string CreatePaymentUrl(Order order, string clientIp)
        {
            if (order.Status != OrderStatus.Pending) throw new InvalidOperationException("Only pending orders can be paid.");
            
            var now     = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _vnTz);
            var txnRef  = $"{order.OrderNumber}_{now:HHmmss}";
            var @params = new SortedDictionary<string, string>
            {
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = _cfg["Command"]!,
                ["vnp_TmnCode"] = _cfg["TmnCode"]!,
                ["vnp_Amount"] = ((long)Math.Round(order.Total * 100)).ToString(),
                ["vnp_CurrCode"] = _cfg["CurrCode"]!,
                ["vnp_TxnRef"] = txnRef,                             
                ["vnp_OrderInfo"] = $"Thanhtoan{order.OrderNumber}",
                ["vnp_OrderType"] = "other",
                ["vnp_Locale"] = _cfg["Locale"]!,
                ["vnp_ReturnUrl"] = _cfg["ReturnUrl"]!,
                ["vnp_IpAddr"] = clientIp,
                ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss"),     
                ["vnp_ExpireDate"] = now.AddMinutes(15).ToString("yyyyMMddHHmmss"),
            };

            var query = BuildQueryString(@params);
            var signature = HmacSHA512(_cfg["HashSecret"]!, query);

            return $"{_cfg["BaseUrl"]}?{query}&vnp_SecureHash={signature}";
        }

        public bool ValidateSignature(IQueryCollection query)
        {
            var receivedHash = query["vnp_SecureHash"].ToString();

            var sorted = new SortedDictionary<string, string>(
                query
                    .Where(kv => kv.Key.StartsWith("vnp_")
                              && kv.Key != "vnp_SecureHash"
                              && kv.Key != "vnp_SecureHashType")
                    .ToDictionary(kv => kv.Key, kv => kv.Value.ToString()));

            var signData = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));

            Console.WriteLine($"[VNPay] SignData : {signData}");
            Console.WriteLine($"[VNPay] Received : {receivedHash}");

            var expected = HmacSHA512(_cfg["HashSecret"]!, signData);
            Console.WriteLine($"[VNPay] Expected : {expected}");

            return receivedHash.Equals(expected, StringComparison.OrdinalIgnoreCase);
        }

        public string GetResponseCode(IQueryCollection query)
            => query["vnp_ResponseCode"].ToString();

        public string GetOrderCode(IQueryCollection query)
        {
            var txnRef = query["vnp_TxnRef"].ToString();
            var underscoreIndex = txnRef.LastIndexOf('_');
            return underscoreIndex > 0 ? txnRef[..underscoreIndex] : txnRef;
        }

        public string GetTransactionId(IQueryCollection query)
            => query["vnp_TransactionNo"].ToString();

        private static string BuildQueryString(SortedDictionary<string, string> @params)
        {
            var sb = new StringBuilder();
            foreach (var kv in @params)
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(kv.Key);
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(kv.Value));
            }
            return sb.ToString();
        }

        private static string HmacSHA512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA512(keyBytes);
            return Convert.ToHexString(hmac.ComputeHash(dataBytes)).ToLower();
        }
    }
}