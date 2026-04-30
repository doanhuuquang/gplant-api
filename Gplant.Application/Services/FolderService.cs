using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Folder;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions.Folder;
using System.Text.RegularExpressions;

namespace Gplant.Application.Services
{
    public class FolderService(IFolderRepository folderRepository) : IFolderService
    {
        private readonly string uploadPath = "wwwroot/uploads";
        private const string DEFAULT_USER_UPLOAD_FOLDER = "user-uploads";

        public async Task<FolderResponse> CreateAsync(CreateFolderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Folder name cannot be empty");

            // Tự động tạo slug từ Name
            var slug = GenerateSlug(request.Name);

            // Validate slug không trùng
            var existingFolder = await folderRepository.GetBySlugAsync(slug);
            if (existingFolder != null)
            {
                throw new FolderAlreadyExistsException($"Folder with name '{request.Name}' already exists");
            }

            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Slug = slug,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                MediaCount = 0
            };

            // Tạo folder vật lý trong wwwroot
            var folderPhysicalPath = Path.Combine(uploadPath, folder.Slug);
            try
            {
                if (!Directory.Exists(folderPhysicalPath))
                {
                    Directory.CreateDirectory(folderPhysicalPath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create folder directory: {ex.Message}", ex);
            }

            await folderRepository.CreateAsync(folder);

            return MapToResponse(folder);
        }

        /// <summary>
        /// Lấy hoặc tạo folder mặc định cho user upload
        /// </summary>
        public async Task<FolderResponse> GetOrCreateUserUploadFolderAsync()
        {
            var folder = await folderRepository.GetBySlugAsync(DEFAULT_USER_UPLOAD_FOLDER);
            
            if (folder != null)
            {
                return MapToResponse(folder);
            }

            // ✅ Tránh tạo lại nếu folder đã tồn tại vật lý
            var folderPhysicalPath = Path.Combine(uploadPath, DEFAULT_USER_UPLOAD_FOLDER);
            if (Directory.Exists(folderPhysicalPath))
            {
                // Folder vật lý tồn tại, chỉ tạo record trong DB
                var folder2 = new Folder
                {
                    Id = Guid.NewGuid(),
                    Name = "User Uploads",
                    Slug = DEFAULT_USER_UPLOAD_FOLDER,
                    CreatedAtUtc = DateTimeOffset.UtcNow,
                    MediaCount = 0
                };

                await folderRepository.CreateAsync(folder2);
                return MapToResponse(folder2);
            }

            // Tạo cả folder vật lý và DB
            var request = new CreateFolderRequest
            {
                Name = "User Uploads"
            };

            return await CreateAsync(request);
        }

        public async Task<FolderResponse> GetByIdAsync(Guid id)
        {
            var folder = await folderRepository.GetByIdAsync(id)
                ?? throw new FolderNotFoundException($"Folder with ID {id} not found");

            return MapToResponse(folder);
        }

        public async Task<FolderResponse> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty");

            var folder = await folderRepository.GetBySlugAsync(slug.ToLower().Trim())
                ?? throw new FolderNotFoundException($"Folder with slug '{slug}' not found");

            return MapToResponse(folder);
        }

        public async Task<List<FolderResponse>> GetAllAsync()
        {
            var folders = await folderRepository.GetAllAsync();
            return [.. folders.Select(MapToResponse)];
        }

        public async Task DeleteAsync(Guid id)
        {
            var folder = await folderRepository.GetByIdAsync(id)
                ?? throw new FolderNotFoundException($"Folder with ID {id} not found");

            // Check if folder has media
            if (folder.MediaCount > 0)
            {
                throw new FolderHasMediaException("Cannot delete folder that contains media. Please delete or move media first.");
            }

            // Xóa folder vật lý từ wwwroot
            var folderPhysicalPath = Path.Combine(uploadPath, folder.Slug);
            try
            {
                if (Directory.Exists(folderPhysicalPath))
                {
                    Directory.Delete(folderPhysicalPath, recursive: true);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete folder directory: {ex.Message}", ex);
            }

            await folderRepository.DeleteAsync(id);
        }

        public async Task UpdateMediaCountAsync(Guid folderId, int increment)
        {
            var folder = await folderRepository.GetByIdAsync(folderId)
                ?? throw new FolderNotFoundException($"Folder with ID {folderId} not found");

            folder.MediaCount += increment;
            await folderRepository.UpdateAsync(folder);
        }

        // Private helpers
        private static string GenerateSlug(string name)
        {
            // Chuyển thành chữ thường
            var slug = name.ToLower().Trim();

            // Xóa diacritics (ă, ê, ô, etc.)
            var normalizedString = slug.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            slug = stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);

            // Thay khoảng trắng bằng dấu gạch
            slug = Regex.Replace(slug, @"\s+", "-");

            // Xóa ký tự không hợp lệ
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

            // Xóa dấu gạch liên tiếp
            slug = Regex.Replace(slug, @"-+", "-");

            // Xóa dấu gạch ở đầu và cuối
            slug = slug.Trim('-');

            return slug;
        }

        private static FolderResponse MapToResponse(Folder folder)
        {
            return new FolderResponse
            {
                Id = folder.Id,
                Name = folder.Name,
                Slug = folder.Slug,
                MediaCount = folder.MediaCount,
                CreatedAtUtc = folder.CreatedAtUtc
            };
        }
    }
}