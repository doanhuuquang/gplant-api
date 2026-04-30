using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Gplant.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gplant.API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController(
        IVNPayService vnPayService,
        IOrderService orderService,
        IPaymentRepository paymentRepository) : ControllerBase
    {
        /// <summary>
        /// VNPay IPN — VNPay gọi server-to-server sau khi user thanh toán
        /// </summary>
        [HttpGet("vnpay-ipn")]
        public async Task<IActionResult> VNPayIPN()
        {
            if (!vnPayService.ValidateSignature(Request.Query))
                return Ok(new { RspCode = "97", Message = "Invalid signature" });

            var orderCode = vnPayService.GetOrderCode(Request.Query);
            var responseCode = vnPayService.GetResponseCode(Request.Query);
            var transactionId = vnPayService.GetTransactionId(Request.Query);

            await orderService.HandleVNPayIPNAsync(orderCode, responseCode, transactionId);

            return Ok(new { RspCode = "00", Message = "Confirmed" });
        }

        /// <summary>
        /// VNPay Return — redirect user về sau khi thanh toán xong
        /// </summary>
        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VNPayReturn()
        {
            if (!vnPayService.ValidateSignature(Request.Query))
                return Redirect("...?status=failed&reason=invalid_signature");

            var code = vnPayService.GetResponseCode(Request.Query);
            var orderCode = vnPayService.GetOrderCode(Request.Query);
            var transactionId = vnPayService.GetTransactionId(Request.Query);

            try
            {
                await orderService.HandleVNPayIPNAsync(orderCode, code, transactionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VNPay Return] HandleIPN error: {ex.Message}");
            }

            var redirectUrl = code == "00"
                ? $"http://localhost:3000/en/shop/order-confirmation?status=success&order={orderCode}"
                : $"http://localhost:3000/en/shop/order-confirmation?status=failed&order={orderCode}&code={code}";

            return Redirect(redirectUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("orders/{orderId}/payments")]
        [Authorize]
        public async Task<IActionResult> GetPaymentHistory(Guid orderId)
        {
            var payments = await paymentRepository.GetByOrderIdAsync(orderId);
            return Ok(new SuccessResponse<List<Payment>>(
                StatusCode: 200,
                Message: "Get payment history successful.",
                Data: payments,
                Timestamp: DateTime.UtcNow
            ));
        }
    }
}