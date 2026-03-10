using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.LightningSale;
using Gplant.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gplant.API.Controllers
{
    [Route("api/lightning-sales")]
    [ApiController]
    public class LightningSaleController(ILightningSaleService saleService) : ControllerBase
    {
        /// <summary>
        /// Get all lightning sales (Admin/Manager only)
        /// </summary>
        [HttpGet("all")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> GetAllSales()
        {
            var sales = await saleService.GetAllAsync();

            var response = new SuccessResponse<List<LightningSaleResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get all lightning sales successful.",
                Data: sales,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get active lightning sales (Public)
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSales()
        {
            var sales = await saleService.GetActiveAsync();

            var response = new SuccessResponse<List<LightningSaleResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get active lightning sales successful.",
                Data: sales,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get upcoming lightning sales (Public)
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingSales()
        {
            var sales = await saleService.GetUpcomingAsync();

            var response = new SuccessResponse<List<LightningSaleResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get upcoming lightning sales successful.",
                Data: sales,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get ongoing lightning sales (Public)
        /// </summary>
        [HttpGet("ongoing")]
        public async Task<IActionResult> GetOngoingSales()
        {
            var sales = await saleService.GetOngoingAsync();

            var response = new SuccessResponse<List<LightningSaleResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get ongoing lightning sales successful.",
                Data: sales,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get current active sale (Public)
        /// </summary>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentActiveSale()
        {
            var sale = await saleService.GetCurrentActiveSaleAsync();

            var response = new SuccessResponse<LightningSaleResponse?>(
                StatusCode: StatusCodes.Status200OK,
                Message: sale != null ? "Get current sale successful." : "No active sale at the moment.",
                Data: sale,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get lightning sale by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetSaleById(Guid id)
        {
            var sale = await saleService.GetByIdAsync(id);

            var response = new SuccessResponse<LightningSaleResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get lightning sale successful.",
                Data: sale,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Create new lightning sale (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> CreateSale(CreateLightningSaleRequest request)
        {
            var sale = await saleService.CreateAsync(request);

            var response = new SuccessResponse<LightningSaleResponse>(
                StatusCode: StatusCodes.Status201Created,
                Message: "Create lightning sale successful.",
                Data: sale,
                Timestamp: DateTime.UtcNow
            );

            return CreatedAtAction(nameof(GetSaleById), new { id = sale.Id }, response);
        }

        /// <summary>
        /// Update lightning sale (Admin/Manager only)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> UpdateSale(Guid id, UpdateLightningSaleRequest request)
        {
            await saleService.UpdateAsync(id, request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Update lightning sale successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Delete lightning sale (Admin/Manager only)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> DeleteSale(Guid id)
        {
            await saleService.DeleteAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Delete lightning sale successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Activate lightning sale (Admin/Manager only)
        /// </summary>
        [HttpPatch("{id:guid}/activate")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> ActivateSale(Guid id)
        {
            await saleService.ActivateAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Lightning sale activated successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Deactivate lightning sale (Admin/Manager only)
        /// </summary>
        [HttpPatch("{id:guid}/deactivate")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> DeactivateSale(Guid id)
        {
            await saleService.DeactivateAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Lightning sale deactivated successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Add item to lightning sale (Admin/Manager only)
        /// </summary>
        [HttpPost("{id:guid}/items")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> AddSaleItem(Guid id, AddSaleItemRequest request)
        {
            var item = await saleService.AddItemAsync(id, request);

            var response = new SuccessResponse<LightningSaleItemResponse>(
                StatusCode: StatusCodes.Status201Created,
                Message: "Add sale item successful.",
                Data: item,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Update sale item (Admin/Manager only)
        /// </summary>
        [HttpPut("items/{itemId:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> UpdateSaleItem(Guid itemId, UpdateSaleItemRequest request)
        {
            await saleService.UpdateItemAsync(itemId, request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Update sale item successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Remove item from sale (Admin/Manager only)
        /// </summary>
        [HttpDelete("items/{itemId:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> RemoveSaleItem(Guid itemId)
        {
            await saleService.RemoveItemAsync(itemId);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Remove sale item successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get lightning sale item by variant (Public)
        /// </summary>
        [HttpGet("items/variant/{plantVariantId:guid}")]
        public async Task<IActionResult> GetLightningSaleItemByVariant(Guid plantVariantId)
        {
            var lightningSaleItem = await saleService.GetLightningSaleItemByVariantAsync(plantVariantId);

            var response = new SuccessResponse<object>(
                StatusCode: StatusCodes.Status200OK,
                Message: lightningSaleItem != null ? "Sale item found" : "No active sale for this variant",
                Data: lightningSaleItem,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
    }
}