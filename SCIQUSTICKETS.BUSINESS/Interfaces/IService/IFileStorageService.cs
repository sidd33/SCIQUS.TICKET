using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Http;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface IFileStorageService
	{
		Task<(string RelativeUrl, string OriginalFileName, long FileSizeBytes)> SaveTicketFileAsync(Guid ticketId, IFormFile file);
		void DeletePhysicalFile(string relativeUrl);
	}
}
