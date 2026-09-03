using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SCIQUSTICKETS.BUSINESS.BusinessModels.SupportPlanDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
    public interface ISupportPlanService
    {
        Task<SupportPlanResponse> CreatePlanAsync(CreateSupportPlanRequest request, string createdByUserId);
        Task<SupportPlanResponse> UpdatePlanAsync(Guid planId, UpdateSupportPlanRequest request, string updatedByUserId);
        Task<List<SupportPlanResponse>> GetAllPlansAsync();
        Task<SupportPlanResponse?> GetPlanByIdAsync(Guid planId);
        
        Task<AccountSupportPlanResponse> AssignPlanToAccountAsync(AssignPlanRequest request, string assignedByUserId);
        Task<List<AccountSupportPlanResponse>> GetAccountPlansAsync(string accountId);
        
        Task<bool> HasAvailableQuotaAsync(string accountId);
        Task<bool> ConsumeQuotaAsync(string accountId, Guid ticketId);
        Task<bool> RefundQuotaAsync(Guid ticketId);
        
        Task ValidateAndExpirePlansAsync();

        // Dedicated Employees
        Task<List<DedicatedEmployeeResponse>> GetDedicatedEmployeesAsync(string accountId);
        Task<DedicatedEmployeeResponse> AssignDedicatedEmployeeAsync(AssignDedicatedEmployeeRequest request);
        Task<bool> RemoveDedicatedEmployeeAsync(Guid accountDedicatedEmployeeId);
    }
}
