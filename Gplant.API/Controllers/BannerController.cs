using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests;
using Gplant.Domain.DTOs.Requests.Banner;
using Gplant.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gplant.API.Controllers
{
    [Route("api/banners")]
    [ApiController]
    public class BannerController(IBannerService bannerService) : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> GetBanners()
        {
            var banners = await bannerService.GetBannersAsync();

            var response = new SuccessResponse<List<BannerResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get banners successful.",
                Data: banners,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveBanners()
        {
            var activeBanners = await bannerService.GetActiveBannersAsync();

            var response = new SuccessResponse<List<BannerResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get active banners successful.",
                Data: activeBanners,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createBannerRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> CreateBanner(CreateBannerRequest createBannerRequest) 
        {
            await bannerService.CreateBannerAsync(createBannerRequest);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Create banner successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateBannerRequest"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> UpdateBanner(Guid id, UpdateBannerRequest updateBannerRequest)
        {
            await bannerService.UpdateBannerAsync(id, updateBannerRequest);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Update banner successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Delete banner
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> DeleteBanner(Guid id)
        {
            await bannerService.DeleteBannerAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Delete banner successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Toggle banner active status
        /// </summary>
        [HttpPatch("{id:guid}/toggle-active")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            await bannerService.ToggleActiveAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Banner status updated successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
    }
}
