using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.CareInstruction;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gplant.API.Controllers
{
    [Route("api/care-instructions")]
    [ApiController]
    public class CareInstructionController(ICareInstructionService careInstructionService) : ControllerBase
    {
        /// <summary>
        /// Get all care instructions
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCareInstructions()
        {
            var careInstructions = await careInstructionService.GetAllAsync();

            var response = new SuccessResponse<List<CareInstructionResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get care instructions successful.",
                Data: careInstructions,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get care instruction by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCareInstructionById(Guid id)
        {
            var careInstruction = await careInstructionService.GetByIdAsync(id);

            var response = new SuccessResponse<CareInstructionResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get care instruction successful.",
                Data: careInstruction,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Create new care instruction (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> CreateCareInstruction(CreateCareInstructionRequest request)
        {
            var careInstruction = await careInstructionService.CreateAsync(request);

            var response = new SuccessResponse<CareInstructionResponse>(
                StatusCode: StatusCodes.Status201Created,
                Message: "Create care instruction successful.",
                Data: careInstruction,
                Timestamp: DateTime.UtcNow
            );

            return CreatedAtAction(nameof(GetCareInstructionById), new { id = careInstruction.Id }, response);
        }

        /// <summary>
        /// Update care instruction (Admin/Manager only)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> UpdateCareInstruction(Guid id, UpdateCareInstructionRequest request)
        {
            await careInstructionService.UpdateAsync(id, request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Update care instruction successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Delete care instruction (Admin/Manager only)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> DeleteCareInstruction(Guid id)
        {
            await careInstructionService.DeleteAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Delete care instruction successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
    }
}