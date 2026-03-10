using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.LightningSale;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions.LightningSale;
using Gplant.Domain.Exceptions.PlantVariant;

namespace Gplant.Application.Services
{
    public class LightningSaleService(
        ILightningSaleRepository saleRepository,
        ILightningSaleItemRepository saleItemRepository,
        IPlantService plantService,
        IPlantVariantService variantService) : ILightningSaleService
    {
        public async Task<LightningSaleResponse> GetByIdAsync(Guid id)
        {
            var sale = await saleRepository.GetByIdAsync(id) ?? throw new LightningSaleNotFoundException($"Lightning sale with ID {id} not found");
            return await MapToResponseAsync(sale);
        }

        public async Task<List<LightningSaleResponse>> GetAllAsync()
        {
            var sales = await saleRepository.GetAllAsync();
            var responses = new List<LightningSaleResponse>();

            foreach (var sale in sales)
            {
                responses.Add(await MapToResponseAsync(sale));
            }

            return responses;
        }

        public async Task<List<LightningSaleResponse>> GetActiveAsync()
        {
            var sales = await saleRepository.GetActiveAsync();
            var responses = new List<LightningSaleResponse>();

            foreach (var sale in sales)
            {
                responses.Add(await MapToResponseAsync(sale));
            }

            return responses;
        }

        public async Task<List<LightningSaleResponse>> GetUpcomingAsync()
        {
            var sales = await saleRepository.GetUpcomingAsync();
            var responses = new List<LightningSaleResponse>();

            foreach (var sale in sales)
            {
                responses.Add(await MapToResponseAsync(sale));
            }

            return responses;
        }

        public async Task<List<LightningSaleResponse>> GetOngoingAsync()
        {
            var sales = await saleRepository.GetOngoingAsync();
            var responses = new List<LightningSaleResponse>();

            foreach (var sale in sales)
            {
                responses.Add(await MapToResponseAsync(sale));
            }

            return responses;
        }

        public async Task<LightningSaleResponse?> GetCurrentActiveSaleAsync()
        {
            var sale = await saleRepository.GetCurrentActiveSaleAsync();
            
            if (sale == null) return null;

            return await MapToResponseAsync(sale);
        }

        public async Task<LightningSaleResponse> CreateAsync(CreateLightningSaleRequest request)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(request.Name)) throw new LightningSaleException("Sale name is required");
            if (string.IsNullOrWhiteSpace(request.Description)) throw new LightningSaleException("Sale description is required");
            if (request.StartDateUtc >= request.EndDateUtc) throw new LightningSaleException("End date must be after start date");
            if (request.EndDateUtc <= DateTime.UtcNow) throw new LightningSaleException("End date must be in the future");

            var sale = new LightningSale
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                StartDateUtc = request.StartDateUtc,
                EndDateUtc = request.EndDateUtc,
                IsActive = false,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

            await saleRepository.CreateAsync(sale);

            return await MapToResponseAsync(sale);
        }

        public async Task UpdateAsync(Guid id, UpdateLightningSaleRequest request)
        {
            var sale = await saleRepository.GetByIdAsync(id) ?? throw new LightningSaleNotFoundException($"Lightning sale with ID {id} not found");

            if (!string.IsNullOrWhiteSpace(request.Name)) sale.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.Description)) sale.Description = request.Description;

            if (request.StartDateUtc.HasValue)
            {
                if (request.StartDateUtc.Value >= sale.EndDateUtc) throw new LightningSaleException("Start date must be before end date");
                sale.StartDateUtc = request.StartDateUtc.Value;
            }

            if (request.EndDateUtc.HasValue)
            {
                if (request.EndDateUtc.Value <= sale.StartDateUtc) throw new LightningSaleException("End date must be after start date");
                if (request.EndDateUtc.Value <= DateTimeOffset.UtcNow) throw new LightningSaleException("End date must be in the future");
                sale.EndDateUtc = request.EndDateUtc.Value;
            }

            if (request.IsActive.HasValue) sale.IsActive = request.IsActive.Value;

            await saleRepository.UpdateAsync(sale);
        }

        public async Task DeleteAsync(Guid id)
        {
            var sale = await saleRepository.GetByIdAsync(id) ?? throw new LightningSaleNotFoundException($"Lightning sale with ID {id} not found");

            // Check if sale is ongoing
            var now = DateTime.UtcNow;
            if (sale.StartDateUtc <= now && sale.EndDateUtc >= now && sale.IsActive) throw new LightningSaleException("Cannot delete an ongoing sale");

            // Cascade delete will handle sale items
            await saleRepository.DeleteAsync(sale);
        }

        public async Task ActivateAsync(Guid id)
        {
            var sale = await saleRepository.GetByIdAsync(id) ?? throw new LightningSaleNotFoundException($"Lightning sale with ID {id} not found");

            if (sale.EndDateUtc <= DateTime.UtcNow) throw new LightningSaleException("Cannot activate expired sale");

            sale.IsActive = true;
            await saleRepository.UpdateAsync(sale);
        }

        public async Task DeactivateAsync(Guid id)
        {
            var sale = await saleRepository.GetByIdAsync(id) ?? throw new LightningSaleNotFoundException($"Lightning sale with ID {id} not found");

            sale.IsActive = false;
            await saleRepository.UpdateAsync(sale);
        }

        public async Task<LightningSaleItemResponse> AddItemAsync(Guid saleId, AddSaleItemRequest request)
        {
            // Validate sale exists
            _ = await saleRepository.GetByIdAsync(saleId) ?? throw new LightningSaleNotFoundException($"Lightning sale with ID {saleId} not found");

            // Validate variant exists
            var variant = await variantService.GetByIdAsync(request.PlantVariantId) ?? throw new PlantVariantException($"Plant variant with ID {request.PlantVariantId} not found");

            // Check if item already exists in this sale
            var existingItem = await saleItemRepository.GetByVariantIdAsync(request.PlantVariantId, saleId);
            if (existingItem != null) throw new LightningSaleException($"Plant variant is already in this sale");

            // Validate prices
            if (request.SalePrice <= 0) throw new LightningSaleException("Sale price must be greater than 0");
            if (request.SalePrice >= variant.Price) throw new LightningSaleException("Sale price must be less than original price");
            
            if (request.QuantityLimit <= 0) throw new LightningSaleException("Quantity limit must be greater than 0");

            var discountPercentage = ((variant.Price - request.SalePrice) / variant.Price) * 100;

            var item = new LightningSaleItem
            {
                Id = Guid.NewGuid(),
                LightningSaleId = saleId,
                PlantVariantId = request.PlantVariantId,
                OriginalPrice = variant.Price,
                SalePrice = request.SalePrice,
                DiscountPercentage = discountPercentage,
                QuantityLimit = request.QuantityLimit,
                QuantitySold = 0,
                IsActive = true,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

            await saleItemRepository.CreateAsync(item);

            return await MapToItemResponseAsync(item);
        }

        public async Task UpdateItemAsync(Guid itemId, UpdateSaleItemRequest request)
        {
            var item = await saleItemRepository.GetByIdAsync(itemId) ?? throw new LightningSaleException($"Sale item with ID {itemId} not found");

            if (request.SalePrice.HasValue)
            {
                if (request.SalePrice.Value <= 0) throw new LightningSaleException("Sale price must be greater than 0");
                if (request.SalePrice.Value >= item.OriginalPrice) throw new LightningSaleException("Sale price must be less than original price");

                item.SalePrice = request.SalePrice.Value;
                item.DiscountPercentage = ((item.OriginalPrice - request.SalePrice.Value) / item.OriginalPrice) * 100;
            }

            if (request.QuantityLimit.HasValue)
            {
                if (request.QuantityLimit.Value < item.QuantitySold) throw new LightningSaleException($"Quantity limit cannot be less than quantity sold ({item.QuantitySold})");
                item.QuantityLimit = request.QuantityLimit.Value;
            }

            if (request.IsActive.HasValue) item.IsActive = request.IsActive.Value;

            await saleItemRepository.UpdateAsync(item);
        }

        public async Task RemoveItemAsync(Guid itemId)
        {
            var item = await saleItemRepository.GetByIdAsync(itemId) ?? throw new LightningSaleException($"Sale item with ID {itemId} not found");

            if (item.QuantitySold > 0) throw new LightningSaleException("Cannot remove item that has already been sold");

            await saleItemRepository.DeleteAsync(item);
        }

        public async Task<LightningSaleItemResponse?> GetLightningSaleItemByVariantAsync(Guid plantVariantId)
        {
            var item = await saleItemRepository.GetByVariantIdAsync(plantVariantId);

            if (item == null || !item.IsActive || item.QuantitySold >= item.QuantityLimit) return null;

            // Verify sale is active and ongoing
            var sale = await saleRepository.GetByIdAsync(item.LightningSaleId);
            if (sale == null || !sale.IsActive) return null;

            var now = DateTimeOffset.UtcNow;
            if (now < sale.StartDateUtc || now > sale.EndDateUtc) return null;

            var saleItemResponse = await MapToItemResponseAsync(item);

            return saleItemResponse;
        }

        public async Task IncrementSoldQuantityAsync(Guid itemId, int quantity)
        {
            var item = await saleItemRepository.GetByIdAsync(itemId) ?? throw new LightningSaleException($"Sale item with ID {itemId} not found");

            if (quantity <= 0) throw new LightningSaleException("Quantity must be greater than 0");
            if (item.QuantitySold + quantity > item.QuantityLimit) throw new SaleItemSoldOutException($"Not enough quantity available. Remaining: {item.QuantityLimit - item.QuantitySold}");

            item.QuantitySold += quantity;
            await saleItemRepository.UpdateAsync(item);
        }

        private async Task<LightningSaleResponse> MapToResponseAsync(LightningSale sale)
        {
            var items = await saleItemRepository.GetBySaleIdAsync(sale.Id);
            var itemResponses = new List<LightningSaleItemResponse>();

            foreach (var item in items)
            {
                itemResponses.Add(await MapToItemResponseAsync(item));
            }

            return new LightningSaleResponse
            {
                Id = sale.Id,
                Name = sale.Name,
                Description = sale.Description,
                StartDateUtc = sale.StartDateUtc,
                EndDateUtc = sale.EndDateUtc,
                IsActive = sale.IsActive,
                Items = itemResponses,
                CreatedAtUtc = sale.CreatedAtUtc,
                UpdatedAtUtc = sale.UpdatedAtUtc
            };
        }

        private async Task<LightningSaleItemResponse> MapToItemResponseAsync(LightningSaleItem item)
        {
            var variant = await variantService.GetByIdAsync(item.PlantVariantId);
            var plant = await plantService.GetByIdAsync(variant.PlantId);

            return new LightningSaleItemResponse
            {
                Id = item.Id,
                LightningSaleId = item.LightningSaleId,
                Plant = plant,
                SalePlantVariant = variant,
                OriginalPrice = item.OriginalPrice,
                SalePrice = item.SalePrice,
                DiscountPercentage = item.DiscountPercentage,
                QuantityLimit = item.QuantityLimit,
                QuantitySold = item.QuantitySold,
                IsActive = item.IsActive,
                CreatedAtUtc = item.CreatedAtUtc,
                UpdatedAtUtc = item.UpdatedAtUtc
            };
        }
    }
}