using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
	public interface IGradeRepository : IGenericRepository<Grade>
	{
		Task<List<Grade>> GetAllAsync(bool? isDeleted);
		Task<bool> SoftDeleteAsync(Guid id);
	}
}