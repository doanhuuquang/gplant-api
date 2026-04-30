using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Banner;
using Gplant.Domain.DTOs.Requests.ShippingAddress;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions.Banner;
using Gplant.Domain.Exceptions.ShippingAddress;
using System.Reflection;

namespace Gplant.Application.Services
{
    public class ShippingAddressService (IShippingAddressRepository shippingAddressRepository) : IShippingAddressService
    {
        public async Task AddShippingAddressAsync(Guid userId, AddShippingAddressRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ShippingName)) throw new CreateBannerException("Shipping Name is required");
            if (string.IsNullOrWhiteSpace(request.ShippingPhone)) throw new CreateBannerException("Shipping Phone is required");
            if (string.IsNullOrWhiteSpace(request.Address)) throw new CreateBannerException("Shipping Address is required");
            if (string.IsNullOrWhiteSpace(request.BuildingName)) throw new CreateBannerException("BuildingName is required");
            if (string.IsNullOrWhiteSpace(request.Longitude)) throw new CreateBannerException("Longitude is required");
            if (string.IsNullOrWhiteSpace(request.Latitude)) throw new CreateBannerException("Latitude is required");

            var newShippingAddress = new ShippingAddress
            {
                UserId = userId,
                ShippingName = request.ShippingName,
                ShippingPhone = request.ShippingPhone,
                Address = request.Address,
                BuildingName = request.BuildingName,
                IsPrimary = request.IsPrimary,
                Longitude = request.Longitude,
                Latitude = request.Latitude,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

            var userAddresses = await shippingAddressRepository.GetShippingAddressesByUserIdAsync(userId);

            if (newShippingAddress.IsPrimary)
            {
                var currentPrimary = userAddresses.FirstOrDefault(a => a.IsPrimary);
                if (currentPrimary != null)
                {
                    currentPrimary.IsPrimary = false;
                    currentPrimary.UpdatedAtUtc = DateTimeOffset.UtcNow;
                    await shippingAddressRepository.UpdateShippingAddressAsync(currentPrimary);
                }
            }
            else
            {
                if (userAddresses.Count == 0)
                {
                    newShippingAddress.IsPrimary = true;
                }
            }

            await shippingAddressRepository.CreateShippingAddressAsync(newShippingAddress);
        }

        public async Task DeleteShippingAddressAsync(Guid shippingAddressId)
        {
            var shippingAddress = await shippingAddressRepository.GetShippingAddressByIdAsync(shippingAddressId) ?? throw new ShippingAddressNotFoundException($"Address with ID {shippingAddressId} not found");

            await shippingAddressRepository.DeleteShippingAddressAsync(shippingAddress);
        }

        public async Task<List<ShippingAddressResponse>> GetShippingAddressesByUserIdAsync(Guid userId)
        {
            var shippingAddresses = await shippingAddressRepository.GetShippingAddressesByUserIdAsync(userId);

            var responses = new List<ShippingAddressResponse>();
            foreach (var shippingAddress in shippingAddresses)
            {
                responses.Add(MapToResponse(shippingAddress));
            }

            return responses;
        }

        public async Task UpdateShippingAddressAsync(Guid id, UpdateShippingAddressRequest request)
        {
            var shippingAddress = await shippingAddressRepository.GetShippingAddressByIdAsync(id) ?? throw new ShippingAddressNotFoundException($"Address with ID {id} not found");
            
            var userAddresses = await shippingAddressRepository.GetShippingAddressesByUserIdAsync(shippingAddress.UserId);

            if (!string.IsNullOrWhiteSpace(request.ShippingName)) shippingAddress.ShippingName = request.ShippingName;
            if (!string.IsNullOrWhiteSpace(request.ShippingPhone)) shippingAddress.ShippingPhone = request.ShippingPhone;
            if (!string.IsNullOrWhiteSpace(request.Address)) shippingAddress.Address = request.Address;
            if (!string.IsNullOrWhiteSpace(request.BuildingName)) shippingAddress.BuildingName = request.BuildingName;
            if (!string.IsNullOrWhiteSpace(request.Longitude)) shippingAddress.Longitude = request.Longitude;
            if (!string.IsNullOrWhiteSpace(request.Latitude)) shippingAddress.Latitude = request.Latitude;
            
            if (request.IsPrimary)
            {
                var otherPrimary = userAddresses.FirstOrDefault(a => a.IsPrimary && a.Id != shippingAddress.Id);
                if (otherPrimary != null)
                {
                    otherPrimary.IsPrimary = false;
                    otherPrimary.UpdatedAtUtc = DateTimeOffset.UtcNow;
                    await shippingAddressRepository.UpdateShippingAddressAsync(otherPrimary);
                }

                shippingAddress.IsPrimary = true;
            }
            else
            {
                if (!request.IsPrimary && shippingAddress.IsPrimary)
                {
                    shippingAddress.IsPrimary = false;
                }
            }

            shippingAddress.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await shippingAddressRepository.UpdateShippingAddressAsync(shippingAddress);
        }

        private static ShippingAddressResponse MapToResponse(ShippingAddress shippingAddress)
        {
            return new ShippingAddressResponse
            {
                Id = shippingAddress.Id,
                UserId = shippingAddress.UserId,
                ShippingName = shippingAddress.ShippingName,
                ShippingPhone = shippingAddress.ShippingPhone,
                Address = shippingAddress.Address,
                BuildingName = shippingAddress.BuildingName,
                IsPrimary = shippingAddress.IsPrimary,
                Longitude = shippingAddress.Longitude,
                Latitude = shippingAddress.Latitude,
                CreatedAtUtc = shippingAddress.CreatedAtUtc,
                UpdatedAtUtc = shippingAddress.UpdatedAtUtc
            };
        }
    }
}
