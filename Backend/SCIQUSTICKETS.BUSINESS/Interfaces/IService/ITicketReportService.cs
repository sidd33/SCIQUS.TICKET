// SCIQUSTICKETS.BUSINESS/Interfaces/IService/ITicketReportService.cs
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface ITicketReportService
	{
		Task<DashboardSummaryResponse> GetSummaryAsync(TicketReportQueryParams query);
		Task<IEnumerable<GroupCountResponse>> GetCountsByDepartmentAsync(TicketReportQueryParams query);
		Task<IEnumerable<GroupCountResponse>> GetCountsByPriorityAsync(TicketReportQueryParams query);
		Task<IEnumerable<GroupCountResponse>> GetCountsByTypeAsync(TicketReportQueryParams query);
		Task<IEnumerable<GroupCountResponse>> GetCountsByStatusAsync(TicketReportQueryParams query);
		Task<SlaComplianceResponse> GetSlaComplianceAsync(TicketReportQueryParams query);
		Task<ReopenRateResponse> GetReopenRateAsync(TicketReportQueryParams query);
		Task<IEnumerable<AgentWorkloadResponse>> GetAgentWorkloadAsync(TicketReportQueryParams query);
		Task<IEnumerable<ChannelMixResponse>> GetChannelMixAsync(TicketReportQueryParams query);
	}
}