using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Cart;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions.Cart;
using Gplant.Domain.Exceptions.Inventory;
using Gplant.Domain.Exceptions.PlantVariant;

namespace Gplant.Application.Services
{
    public class CartService(
        ICartRepository cartRepository,
        ICartItemRepository cartItemRepository,
        IPlantVariantRepository variantRepository,
        IInventoryRepository inventoryRepository,
        IPlantService plantService,
        IPlantVariantService plantVariantService,
        ILightningSaleService lightningSaleService) : ICartService
    {
        public async Task<CartResponse> GetMyCartAsync(Guid userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return await MapToResponseAsync(cart);
        }

        public async Task<CartResponse> AddToCartAsync(Guid userId, AddToCartRequest request)
        {
            // Validate
            if (request.Quantity <= 0) throw new CartException("Quantity must be greater than 0");

            // Check variant exists
            var variant = await variantRepository.GetByIdAsync(request.PlantVariantId) ?? throw new PlantVariantException($"Plant variant with ID {request.PlantVariantId} not found");

            if (!variant.IsActive) throw new PlantVariantException("This plant variant is not available");

            // Check inventory
            var inventory = await inventoryRepository.GetByPlantVariantIdAsync(request.PlantVariantId);
            if (inventory == null || inventory.QuantityAvailable < request.Quantity) throw new InsufficientStockException($"Not enough stock. Available: {inventory?.QuantityAvailable ?? 0}");

            // Get or create cart
            var cart = await GetOrCreateCartAsync(userId);

            // Check if item already exists in cart
            var existingItem = await cartItemRepository.GetByCartAndVariantAsync(cart.Id, request.PlantVariantId);

            if (existingItem != null)
            {
                // Update quantity
                var newQuantity = existingItem.Quantity + request.Quantity;
                
                if (inventory.QuantityAvailable < newQuantity) throw new InsufficientStockException($"Not enough stock. Available: {inventory.QuantityAvailable}");

                existingItem.Quantity = newQuantity;
                await cartItemRepository.UpdateAsync(existingItem);
            }
            else
            {
                // Get sale price if available
                var lightningSaleItemResponse = await lightningSaleService.GetLightningSaleItemByVariantAsync(request.PlantVariantId);

                // Add new item
                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    PlantVariantId = request.PlantVariantId,
                    Quantity = request.Quantity,
                    Price = variant.Price,
                    SalePrice = lightningSaleItemResponse?.SalePrice,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };

                await cartItemRepository.CreateAsync(cartItem);
            }

            // Update cart timestamp
            await cartRepository.UpdateAsync(cart);

            return await MapToResponseAsync(cart);
        }

        public async Task<CartResponse> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var cartItem = await cartItemRepository.GetByIdAsync(cartItemId)
                ?? throw new CartItemNotFoundException($"Cart item with ID {cartItemId} not found");

            // Verify item belongs to user's cart
            if (cartItem.CartId != cart.Id)
                throw new CartException("This item does not belong to your cart");

            if (request.Quantity <= 0)
                throw new CartException("Quantity must be greater than 0");

            // Check inventory
            var inventory = await inventoryRepository.GetByPlantVariantIdAsync(cartItem.PlantVariantId);
            if (inventory == null || inventory.QuantityAvailable < request.Quantity)
                throw new InsufficientStockException($"Not enough stock. Available: {inventory?.QuantityAvailable ?? 0}");

            cartItem.Quantity = request.Quantity;
            await cartItemRepository.UpdateAsync(cartItem);

            // Update cart timestamp
            await cartRepository.UpdateAsync(cart);

            return await MapToResponseAsync(cart);
        }

        public async Task RemoveFromCartAsync(Guid userId, Guid cartItemId)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var cartItem = await cartItemRepository.GetByIdAsync(cartItemId)
                ?? throw new CartItemNotFoundException($"Cart item with ID {cartItemId} not found");

            // Verify item belongs to user's cart
            if (cartItem.CartId != cart.Id)
                throw new CartException("This item does not belong to your cart");

            await cartItemRepository.DeleteAsync(cartItem);

            // Update cart timestamp
            await cartRepository.UpdateAsync(cart);
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            await cartItemRepository.DeleteByCartIdAsync(cart.Id);

            // Update cart timestamp
            await cartRepository.UpdateAsync(cart);
        }

        public async Task<int> GetCartItemCountAsync(Guid userId)
        {
            var cart = await cartRepository.GetByUserIdAsync(userId);
            if (cart == null) return 0;

            var items = await cartItemRepository.GetByCartIdAsync(cart.Id);
            return items.Sum(i => i.Quantity);
        }

        public async Task SyncCartPricesAsync(Guid userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var items = await cartItemRepository.GetByCartIdAsync(cart.Id);

            foreach (var item in items)
            {
                var variant = await variantRepository.GetByIdAsync(item.PlantVariantId);
                if (variant == null) continue;

                // Update price if changed
                if (item.Price != variant.Price)
                {
                    item.Price = variant.Price;
                }

                // Update sale price
                var lightningSaleItemResponse = await lightningSaleService.GetLightningSaleItemByVariantAsync(item.PlantVariantId);
                item.SalePrice = lightningSaleItemResponse?.SalePrice;

                await cartItemRepository.UpdateAsync(item);
            }

            await cartRepository.UpdateAsync(cart);
        }

        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            var cart = await cartRepository.GetByUserIdAsync(userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };

                await cartRepository.CreateAsync(cart);
            }

            return cart;
        }

        private async Task<CartResponse> MapToResponseAsync(Cart cart)
        {
            var items = await cartItemRepository.GetByCartIdAsync(cart.Id);
            var itemResponses = new List<CartItemResponse>();

            foreach (var item in items)
            {
                var variant = await plantVariantService.GetByIdAsync(item.PlantVariantId);
                var plant = variant != null ? await plantService.GetByIdAsync(variant.PlantId) : null;
                var inventory = await inventoryRepository.GetByPlantVariantIdAsync(item.PlantVariantId);

                itemResponses.Add(new CartItemResponse
                {
                    Id = item.Id,
                    CartId = item.CartId,
                    PlantVariantId = item.PlantVariantId,
                    PlantVariant = variant,
                    Plant = plant,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    SalePrice = item.SalePrice,
                    IsInStock = inventory != null && inventory.QuantityAvailable >= item.Quantity,
                    CreatedAtUtc = item.CreatedAtUtc,
                    UpdatedAtUtc = item.UpdatedAtUtc
                });
            }

            return new CartResponse
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = itemResponses,
                CreatedAtUtc = cart.CreatedAtUtc,
                UpdatedAtUtc = cart.UpdatedAtUtc
            };
        }
    }
}