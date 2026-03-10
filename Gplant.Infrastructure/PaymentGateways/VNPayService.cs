using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Gplant.Infrastructure.PaymentGateways
{
    public class VNPayService : IPaymentService
    {
        private readonly VNPayConfig _config;

        public VNPayService(IOptions<VNPayConfig> config)
        {
            _config = config.Value;
        }

        public Task<string> CreatePaymentUrlAsync(Order order, string returnUrl, string ipAddress)
        {
            var vnpay = new VNPayLibrary();
            
            vnpay.AddRequestData("vnp_Version", _config.Version);
            vnpay.AddRequestData("vnp_Command", _config.Command);
            vnpay.AddRequestData("vnp_TmnCode", _config.TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(order.Total * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {order.OrderNumber}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", order.OrderNumber);

            var paymentUrl = vnpay.CreateRequestUrl(_config.Url, _config.HashSecret);
            
            return Task.FromResult(paymentUrl);
        }

        public Task<PaymentCallbackResponse> ProcessPaymentCallbackAsync(Dictionary<string, string> queryParams)
        {
            var vnpay = new VNPayLibrary();
            
            foreach (var (key, value) in queryParams)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            var orderNumber = vnpay.GetResponseData("vnp_TxnRef");
            var transactionId = vnpay.GetResponseData("vnp_TransactionNo");
            var responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var amount = Convert.ToDecimal(vnpay.GetResponseData("vnp_Amount")) / 100;
            
            var isValidSignature = vnpay.ValidateSignature(queryParams["vnp_SecureHash"], _config.HashSecret);
            var isSuccess = responseCode == "00" && isValidSignature;

            var response = new PaymentCallbackResponse
            {
                Success = isSuccess,
                OrderNumber = orderNumber,
                TransactionId = transactionId,
                Message = GetResponseMessage(responseCode),
                Amount = amount,
                PaymentStatus = isSuccess ? PaymentStatus.Paid : PaymentStatus.Failed
            };

            return Task.FromResult(response);
        }

        public bool ValidatePaymentCallback(Dictionary<string, string> queryParams)
        {
            if (!queryParams.ContainsKey("vnp_SecureHash"))
                return false;

            var vnpay = new VNPayLibrary();
            foreach (var (key, value) in queryParams)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            return vnpay.ValidateSignature(queryParams["vnp_SecureHash"], _config.HashSecret);
        }

        private static string GetResponseMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
                "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
                "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
                "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
                "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
                "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
                "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
                "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
                "75" => "Ngân hàng thanh toán đang bảo trì.",
                "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
                _ => "Giao dịch thất bại"
            };
        }
    }

    // VNPay Library Helper
    public class VNPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new();
        private readonly SortedList<string, string> _responseData = new();

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            var data = new StringBuilder();
            foreach (var (key, value) in _requestData)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
                }
            }

            var queryString = data.ToString();
            if (queryString.Length > 0)
            {
                queryString = queryString.Remove(queryString.Length - 1, 1);
            }

            var signData = queryString;
            var vnpSecureHash = HmacSHA512(hashSecret, signData);
            
            return $"{baseUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var data = new StringBuilder();
            foreach (var (key, value) in _responseData)
            {
                if (!string.IsNullOrEmpty(value) && key != "vnp_SecureHash")
                {
                    data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
                }
            }

            var checksum = data.ToString();
            if (checksum.Length > 0)
            {
                checksum = checksum.Remove(checksum.Length - 1, 1);
            }

            var vnpSecureHash = HmacSHA512(secretKey, checksum);
            return vnpSecureHash.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private static string HmacSHA512(string key, string data)
        {
            var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexStringLower(hashValue);
        }
    }
}