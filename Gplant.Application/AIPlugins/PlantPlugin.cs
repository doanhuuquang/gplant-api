using System.ComponentModel;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Plant;
using Microsoft.SemanticKernel;

namespace Gplant.Application.AIPlugins
{
    public class PlantPlugin(IPlantService plantService)
    {
        [KernelFunction("search_plants")]
        [Description("Tìm kiếm các loại cây trong hệ thống. Thường dùng khi khách hàng muốn hỏi shop có bán loại cây nào, hoặc tìm cây theo tên.")]
        public async Task<string> SearchPlantsAsync(
            [Description("Từ khóa để tìm kiếm tên cây, ví dụ: hoa, hồng, trong nhà, văn phòng, bonsai...")] PlantFilterRequest filter)
        {
            var pagedPlants = await plantService.GetPlantsAsync(filter);

            if (pagedPlants.Items.Count == 0) return $"Không tìm thấy loại cây nào trong cửa hàng.";

            var plantInfoList = new List<string>();
            foreach (var plant in pagedPlants.Items)
            {
                var priceInfo = (plant.MinPrice != null && plant.MaxPrice != null)
                    ? $"Giá từ {plant.MinPrice}đ - {plant.MaxPrice}đ"
                    : "Đang cập nhật giá";

                plantInfoList.Add($"- {plant.Name}: {priceInfo}. Mô tả: {plant.ShortDescription}");
            }

            return $"Kết quả tìm kiếm':\n" + string.Join("\n", plantInfoList);
        }
    }
}