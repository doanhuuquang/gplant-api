using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.CareInstruction;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions.CareInstruction;

namespace Gplant.Application.Services
{
    public class CareInstructionService(ICareInstructionRepository repository) : ICareInstructionService
    {
        public async Task<CareInstructionResponse> GetByIdAsync(Guid id)
        {
            var careInstruction = await repository.GetByIdAsync(id)
                ?? throw new CareInstructionException($"Care instruction with ID {id} not found");
            
            return MapToResponseAsync(careInstruction);
        }

        public async Task<List<CareInstructionResponse>> GetAllAsync()
        {
            var careInstructions = await repository.GetAllAsync();
            var responses = new List<CareInstructionResponse>();

            foreach (var careInstruction in careInstructions)
            {
                responses.Add(MapToResponseAsync(careInstruction));
            }
            return responses;
        }

        public async Task<CareInstructionResponse> CreateAsync(CreateCareInstructionRequest request)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(request.LightRequirement))
                throw new CareInstructionException("Light requirement is required");
            if (string.IsNullOrWhiteSpace(request.WateringFrequency))
                throw new CareInstructionException("Watering frequency is required");
            if (string.IsNullOrWhiteSpace(request.Temperature))
                throw new CareInstructionException("Temperature is required");
            if (string.IsNullOrWhiteSpace(request.Soil))
                throw new CareInstructionException("Soil information is required");
            if (string.IsNullOrWhiteSpace(request.Notes))
                throw new CareInstructionException("Notes are required");

            var careInstruction = new CareInstruction
            {
                Id = Guid.NewGuid(),
                LightRequirement = request.LightRequirement,
                WateringFrequency = request.WateringFrequency,
                Temperature = request.Temperature,
                Soil = request.Soil,
                Notes = request.Notes,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

            await repository.CreateAsync(careInstruction);

            return MapToResponseAsync(careInstruction);
        }

        public async Task UpdateAsync(Guid id, UpdateCareInstructionRequest request)
        {
            var careInstruction = await repository.GetByIdAsync(id)
                ?? throw new CareInstructionException($"Care instruction with ID {id} not found");

            if (!string.IsNullOrWhiteSpace(request.LightRequirement))
                careInstruction.LightRequirement = request.LightRequirement;

            if (!string.IsNullOrWhiteSpace(request.WateringFrequency))
                careInstruction.WateringFrequency = request.WateringFrequency;

            if (!string.IsNullOrWhiteSpace(request.Temperature))
                careInstruction.Temperature = request.Temperature;

            if (!string.IsNullOrWhiteSpace(request.Soil))
                careInstruction.Soil = request.Soil;

            if (!string.IsNullOrWhiteSpace(request.Notes))
                careInstruction.Notes = request.Notes;

            careInstruction.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await repository.UpdateAsync(careInstruction);
        }

        public async Task DeleteAsync(Guid id)
        {
            var careInstruction = await repository.GetByIdAsync(id) ?? throw new CareInstructionException($"Care instruction with ID {id} not found");

            // ✅ Kiểm tra có plant nào đang dùng không
            var isUsedByPlants = await repository.IsUsedByPlantsAsync(id);
            if (isUsedByPlants) throw new CareInstructionException("Cannot delete care instruction that is being used by plants. Please reassign or delete the plants first.");

            await repository.DeleteAsync(careInstruction);
        }

        private static CareInstructionResponse MapToResponseAsync(CareInstruction careInstruction)
        {
            return new CareInstructionResponse
            {
                Id = careInstruction.Id,
                LightRequirement = careInstruction.LightRequirement,
                WateringFrequency = careInstruction.WateringFrequency,
                Temperature = careInstruction.Temperature,
                Soil = careInstruction.Soil,
                Notes = careInstruction.Notes,
                CreatedAtUtc = careInstruction.CreatedAtUtc,
                UpdatedAtUtc = careInstruction.UpdatedAtUtc
            };
        }
    }
}