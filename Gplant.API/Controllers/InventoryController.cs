using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Inventory;
using Gplant.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gplant.API.Controllers
{
    [Route("api/inventory")]
    [ApiController]
    public class InventoryController(IInventoryService inventoryService) : ControllerBase
    {
        /// <summary>
        /// Get all inventory items (Admin/Manager only)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> GetAllInventory()
        {
            var inventories = await inventoryService.GetAllAsync();

            var response = new SuccessResponse<List<InventoryResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get all inventory successful.",
                Data: inventories,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get inventory by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> GetInventoryById(Guid id)
        {
            var inventory = await inventoryService.GetByIdAsync(id);

            var response = new SuccessResponse<InventoryResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get inventory successful.",
                Data: inventory,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get inventory by plant variant ID
        /// </summary>
        [HttpGet("by-variant/{plantVariantId:guid}")]
        public async Task<IActionResult> GetInventoryByVariantId(Guid plantVariantId)
        {
            var inventory = await inventoryService.GetByPlantVariantIdAsync(plantVariantId);

            var response = new SuccessResponse<InventoryResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get inventory successful.",
                Data: inventory,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get low stock items (Admin/Manager only)
        /// </summary>
        [HttpGet("low-stock")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> GetLowStockItems([FromQuery] int threshold = 10)
        {
            var inventories = await inventoryService.GetLowStockItemsAsync(threshold);

            var response = new SuccessResponse<List<InventoryResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get low stock items successful.",
                Data: inventories,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get out of stock items (Admin/Manager only)
        /// </summary>
        [HttpGet("out-of-stock")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> GetOutOfStockItems()
        {
            var inventories = await inventoryService.GetOutOfStockItemsAsync();

            var response = new SuccessResponse<List<InventoryResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get out of stock items successful.",
                Data: inventories,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Create new inventory (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> CreateInventory(CreateInventoryRequest request)
        {
            var inventory = await inventoryService.CreateAsync(request);

            var response = new SuccessResponse<InventoryResponse>(
                StatusCode: StatusCodes.Status201Created,
                Message: "Create inventory successful.",
                Data: inventory,
                Timestamp: DateTime.UtcNow
            );

            return CreatedAtAction(nameof(GetInventoryById), new { id = inventory.Id }, response);
        }

        /// <summary>
        /// Update inventory (Admin/Manager only)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> UpdateInventory(Guid id, UpdateInventoryRequest request)
        {
            await inventoryService.UpdateAsync(id, request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Update inventory successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Adjust inventory quantity (Admin/Manager only)
        /// </summary>
        [HttpPatch("{id:guid}/adjust")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> AdjustInventory(Guid id, AdjustInventoryRequest request)
        {
            await inventoryService.AdjustInventoryAsync(id, request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Inventory adjusted successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Reserve inventory (System use - for cart/order)
        /// </summary>
        [HttpPost("reserve")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> ReserveInventory(ReserveInventoryRequest request)
        {
            await inventoryService.ReserveInventoryAsync(request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Inventory reserved successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Check stock availability
        /// </summary>
        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckStockAvailability(
            [FromQuery] Guid plantVariantId,
            [FromQuery] int quantity)
        {
            var isAvailable = await inventoryService.CheckStockAvailabilityAsync(plantVariantId, quantity);

            var response = new SuccessResponse<object>(
                StatusCode: StatusCodes.Status200OK,
                Message: isAvailable ? "Stock available" : "Insufficient stock",
                Data: new { IsAvailable = isAvailable },
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Delete inventory (Admin/Manager only)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> DeleteInventory(Guid id)
        {
            await inventoryService.DeleteAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Delete inventory successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
    }
}