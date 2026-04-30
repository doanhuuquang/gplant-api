using Gplant.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gplant.Application.Services
{
    public class QRPaymentService(IConfiguration config, IHttpClientFactory httpClientFactory) : IQRPaymentService
    {
        private readonly IConfigurationSection _cfg = config.GetSection("VietQR");
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<string> GenerateVietQRCode(decimal amount, string description)
        {
            var clientId = _cfg["ClientId"]!;
            var apiKey = _cfg["ApiKey"]!;
            var accountNo = _cfg["AccountNumber"]!;
            var accountName = _cfg["AccountName"]!;
            var acqId = _cfg["BankBin"]!;

            var payload = new
            {
                accountNo = accountNo,
                accountName = accountName,
                acqId = acqId,
                amount = (int)amount,
                addInfo = description[..Math.Min(description.Length, 25)],
                format = "text",
                template = "compact"
            };

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.vietqr.io/v2/generate");
            request.Headers.Add("x-client-id", clientId);
            request.Headers.Add("x-api-key", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var code = doc.RootElement.GetProperty("code").GetString();

            var qrContent = doc.RootElement.GetProperty("data").GetProperty("qrDataURL").GetString() ?? "";
            return qrContent;
        }
    }
}
