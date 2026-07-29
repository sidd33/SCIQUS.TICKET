using System;
using System.Collections.Generic;
using System.Text;

using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface IGradeService
	{
		Task<GradeResponse?> GetByIdAsync(Guid id);
		Task<List<GradeResponse>> GetAllAsync(bool? isDeleted);
		Task<GradeResponse> CreateAsync(CreateGradeRequest request);
		Task<GradeResponse> UpdateAsync(Guid id, UpdateGradeRequest request);
		Task<bool> SoftDeleteAsync(Guid id);
	}
}
