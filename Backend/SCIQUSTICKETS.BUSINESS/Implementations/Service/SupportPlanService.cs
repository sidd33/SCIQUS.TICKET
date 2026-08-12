using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.BusinessModels.SupportPlanDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.SupportPlanDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class SupportPlanService : ISupportPlanService
    {
        private readonly AppDbContext _context;

        public SupportPlanService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SupportPlanResponse> CreatePlanAsync(CreateSupportPlanRequest request, string createdByUserId)
        {
            var plan = new SupportPlan
            {
                SupportPlanId = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                TicketQuota = request.TicketQuota,
                PeriodType = request.PeriodType,
                ValidityDays = request.ValidityDays,
                BlockWhenExhausted = request.BlockWhenExhausted,
                CreatedDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                CreatedByUserId = createdByUserId
            };

            _context.SupportPlans.Add(plan);
            await _context.SaveChangesAsync();

            return MapToResponse(plan);
        }

        public async Task<SupportPlanResponse> UpdatePlanAsync(Guid planId, UpdateSupportPlanRequest request, string updatedByUserId)
        {
            var plan = await _context.SupportPlans.FindAsync(planId);
            if (plan == null) throw new KeyNotFoundException("Support Plan not found");

            plan.Name = request.Name;
            plan.Description = request.Description;
            plan.TicketQuota = request.TicketQuota;
            plan.PeriodType = request.PeriodType;
            plan.ValidityDays = request.ValidityDays;
            plan.BlockWhenExhausted = request.BlockWhenExhausted;
            plan.Status = request.Status;
            plan.LastUpdatedDate = DateTime.UtcNow;

            _context.SupportPlans.Update(plan);
            await _context.SaveChangesAsync();

            return MapToResponse(plan);
        }

        public async Task<List<SupportPlanResponse>> GetAllPlansAsync()
        {
            var plans = await _context.SupportPlans.ToListAsync();
            return plans.Select(MapToResponse).ToList();
        }

        public async Task<SupportPlanResponse?> GetPlanByIdAsync(Guid planId)
        {
            var plan = await _context.SupportPlans.FindAsync(planId);
            return plan == null ? null : MapToResponse(plan);
        }

        public async Task<AccountSupportPlanResponse> AssignPlanToAccountAsync(AssignPlanRequest request, string assignedByUserId)
        {
            var plan = await _context.SupportPlans.FindAsync(request.SupportPlanId);
            if (plan == null || !plan.Status) throw new InvalidOperationException("Active Support Plan not found");

            // Deactivate existing active plans for the account
            var activePlans = await _context.AccountSupportPlans
                .Where(p => p.AccountId == request.AccountId && p.Status == "Active")
                .ToListAsync();

            foreach (var ap in activePlans)
            {
                ap.Status = "Expired";
                ap.LastUpdatedDate = DateTime.UtcNow;
                _context.AccountSupportPlans.Update(ap);
            }

            var startDate = DateTime.UtcNow;
            DateTime endDate;

            if (plan.ValidityDays.HasValue && plan.ValidityDays.Value > 0)
            {
                endDate = startDate.AddDays(plan.ValidityDays.Value);
            }
            else
            {
                // Default based on period type if no validity days provided
                endDate = plan.PeriodType.ToLower() switch
                {
                    "monthly" => startDate.AddMonths(1),
                    "yearly" => startDate.AddYears(1),
                    _ => startDate.AddYears(1) // Fallback
                };
            }

            var accountPlan = new AccountSupportPlan
            {
                AccountSupportPlanId = Guid.NewGuid(),
                AccountId = request.AccountId,
                SupportPlanId = request.SupportPlanId,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Active",
                CreatedDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                CreatedByUserId = assignedByUserId
            };

            _context.AccountSupportPlans.Add(accountPlan);
            await _context.SaveChangesAsync();

            accountPlan.SupportPlan = plan;
            return MapToAccountPlanResponse(accountPlan, 0);
        }

        public async Task<List<AccountSupportPlanResponse>> GetAccountPlansAsync(string accountId)
        {
            var accountPlans = await _context.AccountSupportPlans
                .Include(ap => ap.SupportPlan)
                .Include(ap => ap.Consumptions)
                .Where(ap => ap.AccountId == accountId)
                .OrderByDescending(ap => ap.CreatedDate)
                .ToListAsync();

            return accountPlans.Select(ap => MapToAccountPlanResponse(ap, ap.Consumptions.Count)).ToList();
        }

        public async Task<bool> HasAvailableQuotaAsync(string accountId)
        {
            var activePlan = await _context.AccountSupportPlans
                .Include(ap => ap.SupportPlan)
                .Include(ap => ap.Consumptions)
                .FirstOrDefaultAsync(ap => ap.AccountId == accountId && ap.Status == "Active");

            if (activePlan == null) return false;

            if (activePlan.EndDate < DateTime.UtcNow)
            {
                // It should have been expired by the background job, but double check
                return false;
            }

            if (activePlan.SupportPlan.BlockWhenExhausted)
            {
                int consumed = activePlan.Consumptions.Count;
                return consumed < activePlan.SupportPlan.TicketQuota;
            }

            return true; // if not blocking, they always have "available" quota logically
        }

        public async Task<bool> ConsumeQuotaAsync(string accountId, Guid ticketId)
        {
            var activePlan = await _context.AccountSupportPlans
                .Include(ap => ap.SupportPlan)
                .Include(ap => ap.Consumptions)
                .FirstOrDefaultAsync(ap => ap.AccountId == accountId && ap.Status == "Active");

            if (activePlan == null || activePlan.EndDate < DateTime.UtcNow) return false;

            int consumed = activePlan.Consumptions.Count;
            if (activePlan.SupportPlan.BlockWhenExhausted && consumed >= activePlan.SupportPlan.TicketQuota)
            {
                return false;
            }

            var consumption = new SupportPlanConsumption
            {
                SupportPlanConsumptionId = Guid.NewGuid(),
                AccountSupportPlanId = activePlan.AccountSupportPlanId,
                TicketId = ticketId,
                ConsumedDate = DateTime.UtcNow
            };

            _context.SupportPlanConsumptions.Add(consumption);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task ValidateAndExpirePlansAsync()
        {
            var now = DateTime.UtcNow;
            
            var expiredPlans = await _context.AccountSupportPlans
                .Include(ap => ap.SupportPlan)
                .Include(ap => ap.Consumptions)
                .Where(ap => ap.Status == "Active")
                .ToListAsync();

            foreach (var plan in expiredPlans)
            {
                bool isExpired = false;

                // 1. Check time expiry
                if (plan.EndDate < now)
                {
                    isExpired = true;
                }
                
                // 2. Check quota expiry
                if (!isExpired && plan.SupportPlan.BlockWhenExhausted)
                {
                    if (plan.Consumptions.Count >= plan.SupportPlan.TicketQuota)
                    {
                        isExpired = true;
                    }
                }

                if (isExpired)
                {
                    plan.Status = "Expired";
                    plan.LastUpdatedDate = now;
                    _context.AccountSupportPlans.Update(plan);
                }
            }

            await _context.SaveChangesAsync();
        }

        private static SupportPlanResponse MapToResponse(SupportPlan plan)
        {
            return new SupportPlanResponse
            {
                SupportPlanId = plan.SupportPlanId,
                Name = plan.Name,
                Description = plan.Description,
                TicketQuota = plan.TicketQuota,
                PeriodType = plan.PeriodType,
                ValidityDays = plan.ValidityDays,
                BlockWhenExhausted = plan.BlockWhenExhausted,
                Status = plan.Status
            };
        }

        private static AccountSupportPlanResponse MapToAccountPlanResponse(AccountSupportPlan accountPlan, int consumedCount)
        {
            return new AccountSupportPlanResponse
            {
                AccountSupportPlanId = accountPlan.AccountSupportPlanId,
                AccountId = accountPlan.AccountId,
                SupportPlanId = accountPlan.SupportPlanId,
                PlanName = accountPlan.SupportPlan?.Name ?? "Unknown Plan",
                StartDate = accountPlan.StartDate,
                EndDate = accountPlan.EndDate,
                Status = accountPlan.Status,
                TicketQuota = accountPlan.SupportPlan?.TicketQuota ?? 0,
                ConsumedQuota = consumedCount,
                RemainingQuota = Math.Max(0, (accountPlan.SupportPlan?.TicketQuota ?? 0) - consumedCount)
            };
        }
    }
}
