using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class GradeService : IGradeService
	{
		private readonly IGradeRepository _gradeRepository;

		public GradeService(IGradeRepository gradeRepository)
		{
			_gradeRepository = gradeRepository;
		}

		public async Task<GradeResponse?> GetByIdAsync(Guid id)
		{
			var grade = await _gradeRepository.GetByIdAsync(id);
			return grade == null ? null : MapToResponse(grade);
		}

		public async Task<List<GradeResponse>> GetAllAsync(bool? isDeleted)
		{
			var grades = await _gradeRepository.GetAllAsync(isDeleted);
			return grades.Select(MapToResponse).ToList();
		}

		public async Task<GradeResponse> CreateAsync(CreateGradeRequest request)
		{
			var grade = new Grade
			{
				Id = Guid.NewGuid(),
				GradeLevel = request.GradeLevel,
				Description = request.Description,
				CreatedDate = DateTime.UtcNow,
				LastUpdatedDate = DateTime.UtcNow
			};

			await _gradeRepository.AddAsync(grade);
			await _gradeRepository.SaveChangesAsync();
			return MapToResponse(grade);
		}

		public async Task<GradeResponse> UpdateAsync(Guid id, UpdateGradeRequest request)
		{
			var grade = await _gradeRepository.GetByIdAsync(id)
				?? throw new KeyNotFoundException($"Grade {id} not found.");

			if (request.GradeLevel.HasValue) grade.GradeLevel = request.GradeLevel.Value;
			if (request.Description != null) grade.Description = request.Description;
			grade.LastUpdatedDate = DateTime.UtcNow;

			_gradeRepository.Update(grade);
			await _gradeRepository.SaveChangesAsync();
			return MapToResponse(grade);
		}

		public async Task<bool> SoftDeleteAsync(Guid id)
		{
			return await _gradeRepository.SoftDeleteAsync(id);
		}

		private static GradeResponse MapToResponse(Grade g) => new()
		{
			Id = g.Id,
			GradeLevel = g.GradeLevel,
			Description = g.Description
		};
	}
}