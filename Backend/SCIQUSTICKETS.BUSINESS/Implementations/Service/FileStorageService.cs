using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class FileStorageService : IFileStorageService
	{
		private readonly IWebHostEnvironment _env;
		private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx", ".txt", ".zip" };
		private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

		public FileStorageService(IWebHostEnvironment env)
		{
			_env = env;
		}

		public async Task<(string RelativeUrl, string OriginalFileName, long FileSizeBytes)> SaveTicketFileAsync(Guid ticketId, IFormFile file)
		{
			if (file == null || file.Length == 0)
				throw new InvalidOperationException("The uploaded file is empty.");
			if (file.Length > MaxFileSizeBytes)
				throw new InvalidOperationException("The uploaded file exceeds the maximum allowed size (10 MB).");

			var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
			if (!AllowedExtensions.Contains(ext))
				throw new InvalidOperationException($"File type '{ext}' is not allowed.");

			var folder = Path.Combine(_env.WebRootPath ?? "wwwroot", "Uploads", "Tickets", ticketId.ToString());
			Directory.CreateDirectory(folder);

			var storedName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
			var fullPath = Path.Combine(folder, storedName);

			await using (var stream = new FileStream(fullPath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			var relativeUrl = $"/Uploads/Tickets/{ticketId}/{storedName}";
			return (relativeUrl, file.FileName, file.Length);
		}

		public void DeletePhysicalFile(string relativeUrl)
		{
			if (string.IsNullOrWhiteSpace(relativeUrl)) return;

			var root = _env.WebRootPath ?? "wwwroot";
			var fullPath = Path.Combine(root, relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
			if (File.Exists(fullPath))
				File.Delete(fullPath);
		}
	}
}