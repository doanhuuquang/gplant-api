using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions.Folder;
using Gplant.Domain.Exceptions.Media;
using Gplant.Domain.Exceptions.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Gplant.Application.Services
{
    public class MediaService(IMediaRepository mediaRepository, IFolderService folderService, UserManager<User> userManager) : IMediaService
    {
        private readonly string uploadPath = "wwwroot/uploads";
        private readonly string baseUrl = "/uploads";

        public async Task<MediaResponse> UploadImageAsync(IFormFile file, Guid uploadedBy, Guid folderId)
        {
            _ = await folderService.GetByIdAsync(folderId) ?? throw new FolderNotFoundException($"Folder with ID {folderId} not found");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension)) throw new InvalidFileException("Only image files (.jpg, .jpeg, .png, .webp, .gif, .svg) are allowed");
            if (file.Length > 5 * 1024 * 1024) throw new InvalidFileException("File size must be less than 5MB");
            if (!file.ContentType.StartsWith("image/")) throw new InvalidFileException("Invalid image file");

            string fileHash;
            using (var stream = file.OpenReadStream())
            {
                fileHash = await ComputeFileHashAsync(stream);
                stream.Position = 0;
            }

            var existingMedia = await mediaRepository.GetByFileHashAsync(fileHash);
            if (existingMedia != null)
            {
                return await MapToResponseAsync(existingMedia);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month;
            var folderPath = $"images/{year}/{month:D2}";
            var relativePath = $"{folderPath}/{fileName}";
            var fullPath = Path.Combine(uploadPath, relativePath);

            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var media = new Media
            {
                Id = Guid.NewGuid(),
                FolderId = folderId,
                FileName = file.FileName,
                FilePath = relativePath,
                FileUrl = $"{baseUrl}/{relativePath}",
                FileSize = file.Length,
                MimeType = file.ContentType,
                FileHash = fileHash,
                UploadedBy = uploadedBy,
                CreatedAtUtc = DateTime.UtcNow,
                IsDeleted = false
            };

            await mediaRepository.CreateAsync(media);

            await folderService.UpdateMediaCountAsync(folderId, 1);

            return await MapToResponseAsync(media);
        }

        public async Task<MediaResponse> GetByIdAsync(Guid id)
        {
            var media = await mediaRepository.GetByIdAsync(id) ?? throw new MediaNotFoundException($"Media with ID {id} not found");

            return await MapToResponseAsync(media);
        }

        public async Task<PagedResult<MediaResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 50, string? searchTerm = null)
        {
            var medias = await mediaRepository.GetAllAsync(pageNumber, pageSize, searchTerm);
            var totalCount = await mediaRepository.GetTotalCountAsync(searchTerm);

            var items = new List<MediaResponse>();
            foreach (var media in medias)
            {
                items.Add(await MapToResponseAsync(media));
            }

            return new PagedResult<MediaResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<MediaResponse>> GetByFolderIdAsync(Guid folderId, int pageNumber = 1, int pageSize = 50)
        {
            await folderService.GetByIdAsync(folderId);

            var medias = await mediaRepository.GetByFolderIdAsync(folderId, pageNumber, pageSize);
            var totalCount = await mediaRepository.GetTotalCountByFolderAsync(folderId);

            var items = new List<MediaResponse>();
            foreach (var media in medias)
            {
                items.Add(await MapToResponseAsync(media));
            }

            return new PagedResult<MediaResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            var media = await mediaRepository.GetByIdAsync(id)
                ?? throw new MediaNotFoundException($"Media with ID {id} not found");

            // Check if media is in use
            var isInUse = await mediaRepository.IsMediaInUseAsync(id);
            if (isInUse)
            {
                throw new MediaInUseException("Cannot delete media that is currently in use");
            }

            // Soft delete in DB
            await mediaRepository.DeleteAsync(id);

            // Update folder media count
            await folderService.UpdateMediaCountAsync(media.FolderId, -1);

            // Delete physical file
            var fullPath = Path.Combine(uploadPath, media.FilePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        // Private helpers
        private static async Task<string> ComputeFileHashAsync(Stream stream)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return Convert.ToHexStringLower(hashBytes);
        }

        private async Task<MediaResponse> MapToResponseAsync(Media media)
        {
            var user = await userManager.FindByIdAsync(media.UploadedBy.ToString()) ?? throw new UserNotExistsException("User not found.");
            var roles = await userManager.GetRolesAsync(user);

            var userResponse = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                ProfilePictureUrl = user.ProfilePictureUrl ?? "",
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber ?? "",
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                Roles = [.. roles],
            };

            return new MediaResponse
            {
                Id = media.Id,
                FileName = media.FileName,
                FileUrl = media.FileUrl,
                FileSize = media.FileSize,
                MimeType = media.MimeType,
                Width = media.Width,
                Height = media.Height,
                UploadedBy = userResponse,
                CreatedAtUtc = media.CreatedAtUtc
            };
        }
    }
}