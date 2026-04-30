using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Plant;
using Gplant.Domain.DTOs.Requests.ShippingAddress;
using Gplant.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gplant.API.Controllers
{
    [Route("api/shipping-addresses")]
    [ApiController]
    public class ShippingAddressController (IShippingAddressService shippingAddressService) : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetShippingAddressesByUserId(Guid userId)
        {
            var shippingAddresses = await shippingAddressService.GetShippingAddressesByUserIdAsync(userId);

            var response = new SuccessResponse<List<ShippingAddressResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get Shipping Addresses successful.",
                Data: shippingAddresses,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{userId:guid}")]
        public async Task<IActionResult> CreateShippingAddress(Guid userId, AddShippingAddressRequest request)
        {
            await shippingAddressService.AddShippingAddressAsync(userId, request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status201Created,
                Message: "Create Shipping Address successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shippingAddressId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{shippingAddressId:guid}")]
        public async Task<IActionResult> UpdateShippingAddress(Guid shippingAddressId, UpdateShippingAddressRequest request)
        {
            await shippingAddressService.UpdateShippingAddressAsync(shippingAddressId, request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status201Created,
                Message: "Update Shipping Address successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shippingAddressId"></param>
        /// <returns></returns>
        [HttpDelete("{shippingAddressId:guid}")]
        public async Task<IActionResult> DeleteShippingAddress(Guid shippingAddressId)
        {
            await shippingAddressService.DeleteShippingAddressAsync(shippingAddressId);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Delete Shipping Address successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
    }
}
