using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Inventory;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions.Inventory;
using Gplant.Domain.Exceptions.PlantVariant;

namespace Gplant.Application.Services
{
    public class InventoryService(
        IInventoryRepository inventoryRepository,
        IPlantVariantRepository variantRepository) : IInventoryService
    {
        public async Task<InventoryResponse> GetByIdAsync(Guid id)
        {
            var inventory = await inventoryRepository.GetByIdAsync(id) ?? throw new InventoryNotFoundException($"Inventory with ID {id} not found");
            return await MapToResponseAsync(inventory);
        }

        public async Task<InventoryResponse> GetByPlantVariantIdAsync(Guid plantVariantId)
        {
            var inventory = await inventoryRepository.GetByPlantVariantIdAsync(plantVariantId) ?? throw new InventoryNotFoundException($"Inventory for plant variant {plantVariantId} not found");
            return await MapToResponseAsync(inventory);
        }

        public async Task<List<InventoryResponse>> GetAllAsync()
        {
            var inventories = await inventoryRepository.GetAllAsync();
            var responses = new List<InventoryResponse>();

            foreach (var inventory in inventories)
            {
                responses.Add(await MapToResponseAsync(inventory));
            }

            return responses;
        }

        public async Task<List<InventoryResponse>> GetLowStockItemsAsync(int threshold = 10)
        {
            var inventories = await inventoryRepository.GetLowStockItemsAsync(threshold);
            var responses = new List<InventoryResponse>();

            foreach (var inventory in inventories)
            {
                responses.Add(await MapToResponseAsync(inventory));
            }

            return responses;
        }

        public async Task<List<InventoryResponse>> GetOutOfStockItemsAsync()
        {
            var inventories = await inventoryRepository.GetOutOfStockItemsAsync();
            var responses = new List<InventoryResponse>();

            foreach (var inventory in inventories)
            {
                responses.Add(await MapToResponseAsync(inventory));
            }

            return responses;
        }

        public async Task<InventoryResponse> CreateAsync(CreateInventoryRequest request)
        {
            // Validate
            _ = await variantRepository.GetByIdAsync(request.PlantVariantId) ?? throw new PlantVariantException($"Plant variant with ID {request.PlantVariantId} not found");

            // Check if inventory already exists for this variant
            var existingInventory = await inventoryRepository.GetByPlantVariantIdAsync(request.PlantVariantId);
            
            if (existingInventory != null) throw new InventoryException($"Inventory already exists for plant variant {request.PlantVariantId}");
            if (request.QuantityAvailable < 0) throw new InventoryException("Quantity available cannot be negative");

            var inventory = new Inventory
            {
                Id = Guid.NewGuid(),
                PlantVariantId = request.PlantVariantId,
                QuantityAvailable = request.QuantityAvailable,
                QuantityReserved = 0,
                LastUpdatedAtUtc = DateTimeOffset.UtcNow
            };

            await inventoryRepository.CreateAsync(inventory);

            return await MapToResponseAsync(inventory);
        }

        public async Task UpdateAsync(Guid id, UpdateInventoryRequest request)
        {
            var inventory = await inventoryRepository.GetByIdAsync(id)
                ?? throw new InventoryNotFoundException($"Inventory with ID {id} not found");

            if (request.QuantityAvailable.HasValue)
            {
                if (request.QuantityAvailable.Value < 0) throw new InventoryException("Quantity available cannot be negative");
                inventory.QuantityAvailable = request.QuantityAvailable.Value;
            }

            inventory.LastUpdatedAtUtc = DateTimeOffset.UtcNow;

            await inventoryRepository.UpdateAsync(inventory);
        }

        public async Task DeleteAsync(Guid id)
        {
            var inventory = await inventoryRepository.GetByIdAsync(id) ?? throw new InventoryNotFoundException($"Inventory with ID {id} not found");

            if (inventory.QuantityReserved > 0) throw new InventoryException("Cannot delete inventory with reserved stock");

            await inventoryRepository.DeleteAsync(inventory);
        }

        public async Task AdjustInventoryAsync(Guid id, AdjustInventoryRequest request)
        {
            var inventory = await inventoryRepository.GetByIdAsync(id) ?? throw new InventoryNotFoundException($"Inventory with ID {id} not found");

            var newQuantity = inventory.QuantityAvailable + request.Quantity;

            if (newQuantity < 0) throw new InventoryException($"Insufficient stock. Available: {inventory.QuantityAvailable}, Requested adjustment: {request.Quantity}");

            inventory.QuantityAvailable = newQuantity;

            await inventoryRepository.UpdateAsync(inventory);
        }

        public async Task ReserveInventoryAsync(ReserveInventoryRequest request)
        {
            var inventory = await inventoryRepository.GetByPlantVariantIdAsync(request.PlantVariantId) ?? throw new InventoryNotFoundException($"Inventory for plant variant {request.PlantVariantId} not found");

            if (request.Quantity <= 0) throw new InventoryException("Quantity must be greater than 0");

            if (inventory.QuantityAvailable < request.Quantity) throw new InsufficientStockException($"Insufficient stock. Available: {inventory.QuantityAvailable}, Requested: {request.Quantity}");

            inventory.QuantityAvailable -= request.Quantity;
            inventory.QuantityReserved += request.Quantity;

            await inventoryRepository.UpdateAsync(inventory);
        }

        public async Task ReleaseReservedInventoryAsync(Guid plantVariantId, int quantity)
        {
            var inventory = await inventoryRepository.GetByPlantVariantIdAsync(plantVariantId) ?? throw new InventoryNotFoundException($"Inventory for plant variant {plantVariantId} not found");

            if (quantity <= 0) throw new InventoryException("Quantity must be greater than 0");

            if (inventory.QuantityReserved < quantity) throw new InventoryException($"Cannot release {quantity} items. Only {inventory.QuantityReserved} items are reserved");

            inventory.QuantityReserved -= quantity;
            inventory.QuantityAvailable += quantity;

            await inventoryRepository.UpdateAsync(inventory);
        }

        public async Task<bool> CheckStockAvailabilityAsync(Guid plantVariantId, int quantity)
        {
            var inventory = await inventoryRepository.GetByPlantVariantIdAsync(plantVariantId);
            
            if (inventory == null) return false;
            
            return inventory.QuantityAvailable >= quantity;
        }

        private async Task<InventoryResponse> MapToResponseAsync(Inventory inventory)
        {
            var variant = await variantRepository.GetByIdAsync(inventory.PlantVariantId);

            return new InventoryResponse
            {
                Id = inventory.Id,
                PlantVariantId = inventory.PlantVariantId,
                PlantVariant = variant,
                QuantityAvailable = inventory.QuantityAvailable,
                QuantityReserved = inventory.QuantityReserved,
                LastUpdatedAtUtc = inventory.LastUpdatedAtUtc
            };
        }
    }
}