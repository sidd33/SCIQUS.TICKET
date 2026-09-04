using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.BusinessModels.SupportPlanDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.SupportPlanDATA;
using SCIQUSTICKETS.COMMON.Helpers;

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
                CreatedDate = TimeHelper.GetIndianTime(),
                LastUpdatedDate = TimeHelper.GetIndianTime(),
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
            plan.LastUpdatedDate = TimeHelper.GetIndianTime();

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
                ap.LastUpdatedDate = TimeHelper.GetIndianTime();
                _context.AccountSupportPlans.Update(ap);
            }

            var startDate = TimeHelper.GetIndianTime();
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
                CreatedDate = TimeHelper.GetIndianTime(),
                LastUpdatedDate = TimeHelper.GetIndianTime(),
                CreatedByUserId = assignedByUserId
            };

            _context.AccountSupportPlans.Add(accountPlan);
            await _context.SaveChangesAsync();

            accountPlan.SupportPlan = plan;
            return MapToAccountPlanResponse(accountPlan, 0);
        }

        public async Task<AccountSupportPlanResponse> CreateCustomPlanForAccountAsync(CreateCustomPlanForAccountRequest request, string assignedByUserId)
        {
            var account = await _context.Accounts.FindAsync(request.AccountId);
            if (account == null) throw new KeyNotFoundException("Account not found.");

            // Create dedicated custom SupportPlan record
            var customPlan = new SupportPlan
            {
                SupportPlanId = Guid.NewGuid(),
                Name = string.IsNullOrWhiteSpace(request.CustomPlanName) ? "Custom Plan" : request.CustomPlanName.Trim(),
                Description = $"Custom plan created for {account.AccountName}",
                TicketQuota = request.TicketQuota,
                PeriodType = "Custom",
                ValidityDays = request.ValidityDays > 0 ? request.ValidityDays : 30,
                BlockWhenExhausted = request.BlockWhenExhausted,
                SupportHours = request.SupportHours ?? "StandardBusinessHours",
                IncludesWeekendSupport = request.IncludesWeekendSupport,
                Status = true,
                CreatedDate = TimeHelper.GetIndianTime(),
                LastUpdatedDate = TimeHelper.GetIndianTime(),
                CreatedByUserId = assignedByUserId
            };

            _context.SupportPlans.Add(customPlan);

            // Deactivate any existing active plans for this account
            var activePlans = await _context.AccountSupportPlans
                .Where(ap => ap.AccountId == request.AccountId && ap.Status == "Active")
                .ToListAsync();

            foreach (var ap in activePlans)
            {
                ap.Status = "Expired";
                ap.LastUpdatedDate = TimeHelper.GetIndianTime();
                _context.AccountSupportPlans.Update(ap);
            }

            var startDate = TimeHelper.GetIndianTime();
            var endDate = startDate.AddDays(customPlan.ValidityDays.Value);

            var accountPlan = new AccountSupportPlan
            {
                AccountSupportPlanId = Guid.NewGuid(),
                AccountId = request.AccountId,
                SupportPlanId = customPlan.SupportPlanId,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Active",
                CreatedDate = TimeHelper.GetIndianTime(),
                LastUpdatedDate = TimeHelper.GetIndianTime(),
                CreatedByUserId = assignedByUserId
            };

            _context.AccountSupportPlans.Add(accountPlan);
            await _context.SaveChangesAsync();

            accountPlan.SupportPlan = customPlan;
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

            var now = TimeHelper.GetIndianTime();
            return accountPlans.Select(ap => {
                var periodStart = GetPeriodStartDate(ap.StartDate, ap.SupportPlan?.PeriodType ?? "", now);
                int consumedCount = ap.Consumptions.Count(c => !c.IsRefunded && c.ConsumedDate >= periodStart);
                return MapToAccountPlanResponse(ap, consumedCount);
            }).ToList();
        }

        public async Task<bool> HasAvailableQuotaAsync(string accountId)
        {
            var activePlan = await _context.AccountSupportPlans
                .Include(ap => ap.SupportPlan)
                .Include(ap => ap.Consumptions)
                .FirstOrDefaultAsync(ap => ap.AccountId == accountId && ap.Status == "Active");

            if (activePlan == null) return false;

            var now = TimeHelper.GetIndianTime();
            if (activePlan.EndDate < now)
            {
                // It should have been expired by the background job, but double check
                return false;
            }

            if (activePlan.SupportPlan.BlockWhenExhausted)
            {
                var periodStart = GetPeriodStartDate(activePlan.StartDate, activePlan.SupportPlan.PeriodType, now);
                int consumed = activePlan.Consumptions.Count(c => !c.IsRefunded && c.ConsumedDate >= periodStart);
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

            var now = TimeHelper.GetIndianTime();
            if (activePlan == null || activePlan.EndDate < now) return false;

            var periodStart = GetPeriodStartDate(activePlan.StartDate, activePlan.SupportPlan.PeriodType, now);
            int consumed = activePlan.Consumptions.Count(c => !c.IsRefunded && c.ConsumedDate >= periodStart);
            
            bool isOverage = consumed >= activePlan.SupportPlan.TicketQuota;

            if (activePlan.SupportPlan.BlockWhenExhausted && isOverage)
            {
                return false;
            }

            var consumption = new SupportPlanConsumption
            {
                SupportPlanConsumptionId = Guid.NewGuid(),
                AccountSupportPlanId = activePlan.AccountSupportPlanId,
                TicketId = ticketId,
                ConsumedDate = now,
                IsOverage = isOverage
            };

            _context.SupportPlanConsumptions.Add(consumption);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RefundQuotaAsync(Guid ticketId)
        {
            var consumption = await _context.SupportPlanConsumptions
                .FirstOrDefaultAsync(c => c.TicketId == ticketId && !c.IsRefunded);

            if (consumption == null) return false;

            consumption.IsRefunded = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task ValidateAndExpirePlansAsync()
        {
            var now = TimeHelper.GetIndianTime();
            
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
                    var periodType = plan.SupportPlan.PeriodType?.ToLower();
                    if (periodType == "lifetime" || periodType == "one-time" || string.IsNullOrEmpty(periodType))
                    {
                        if (plan.Consumptions.Count(c => !c.IsRefunded) >= plan.SupportPlan.TicketQuota)
                        {
                            isExpired = true;
                        }
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

        public async Task<List<DedicatedEmployeeResponse>> GetDedicatedEmployeesAsync(string accountId)
        {
            var employees = await _context.AccountDedicatedEmployees
                .Where(ade => ade.AccountId == accountId)
                .Select(ade => new DedicatedEmployeeResponse
                {
                    AccountDedicatedEmployeeId = ade.AccountDedicatedEmployeeId,
                    EmployeeUserId = ade.EmployeeUserId,
                    // Note: Ideally join with Users table, but since Employee.Name is available, we could fetch from Employee.
                    // For now, returning IDs. We will fetch Name in the Controller or UI, or include Employee relation if configured.
                })
                .ToListAsync();
            
            // Let's manually fetch names if Employee table is mapped
            var employeeIds = employees.Select(e => e.EmployeeUserId).ToList();
            var employeeDetails = await _context.Employees
                .Where(e => employeeIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, e => new { e.Name, e.Email });

            foreach (var emp in employees)
            {
                if (employeeDetails.TryGetValue(emp.EmployeeUserId, out var details))
                {
                    emp.EmployeeName = details.Name;
                    emp.EmployeeEmail = details.Email;
                }
            }

            return employees;
        }

        public async Task<DedicatedEmployeeResponse> AssignDedicatedEmployeeAsync(AssignDedicatedEmployeeRequest request)
        {
            var activeAccountPlan = await _context.AccountSupportPlans
                .Include(ap => ap.SupportPlan)
                .FirstOrDefaultAsync(ap => ap.AccountId == request.AccountId && ap.Status == "Active");

            if (activeAccountPlan == null || activeAccountPlan.SupportPlan == null)
            {
                throw new InvalidOperationException("Account does not have an active support plan. Dedicated employees require a Gold or Platinum plan.");
            }

            var planName = activeAccountPlan.SupportPlan.Name?.ToLower() ?? "";
            if (planName.Contains("basic") || planName.Contains("silver"))
            {
                throw new InvalidOperationException($"Dedicated employees are not available on the {activeAccountPlan.SupportPlan.Name} plan. Upgrade to Gold or Platinum.");
            }

            var currentDedicatedCount = await _context.AccountDedicatedEmployees.CountAsync(ade => ade.AccountId == request.AccountId);

            if (planName.Contains("platinum") && currentDedicatedCount >= 1)
            {
                throw new InvalidOperationException("Platinum plan allows a maximum of 1 dedicated 24/7 employee.");
            }

            if (planName.Contains("gold") && currentDedicatedCount >= 3)
            {
                throw new InvalidOperationException("Gold plan allows a maximum of 3 dedicated employees.");
            }

            var existing = await _context.AccountDedicatedEmployees
                .FirstOrDefaultAsync(ade => ade.AccountId == request.AccountId && ade.EmployeeUserId == request.EmployeeUserId);
            
            if (existing != null) throw new InvalidOperationException("Employee is already dedicated to this account.");

            var newAssign = new AccountDedicatedEmployee
            {
                AccountDedicatedEmployeeId = Guid.NewGuid(),
                AccountId = request.AccountId,
                EmployeeUserId = request.EmployeeUserId
            };

            _context.AccountDedicatedEmployees.Add(newAssign);
            await _context.SaveChangesAsync();

            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeUserId);

            return new DedicatedEmployeeResponse
            {
                AccountDedicatedEmployeeId = newAssign.AccountDedicatedEmployeeId,
                EmployeeUserId = newAssign.EmployeeUserId,
                EmployeeName = emp?.Name ?? "Unknown",
                EmployeeEmail = emp?.Email ?? ""
            };
        }

        public async Task<bool> RemoveDedicatedEmployeeAsync(Guid accountDedicatedEmployeeId)
        {
            var existing = await _context.AccountDedicatedEmployees.FindAsync(accountDedicatedEmployeeId);
            if (existing == null) return false;

            _context.AccountDedicatedEmployees.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
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

        private static DateTime GetPeriodStartDate(DateTime planStart, string periodType, DateTime now)
        {
            if (string.IsNullOrEmpty(periodType)) return planStart;
            
            periodType = periodType.ToLower();
            if (periodType == "monthly")
            {
                var months = ((now.Year - planStart.Year) * 12) + now.Month - planStart.Month;
                if (now.Day < planStart.Day) months--;
                if (months < 0) return planStart;
                return planStart.AddMonths(months);
            }
            if (periodType == "yearly")
            {
                var years = now.Year - planStart.Year;
                if (now < planStart.AddYears(years)) years--;
                if (years < 0) return planStart;
                return planStart.AddYears(years);
            }
            if (periodType == "weekly")
            {
                var diff = (now - planStart).Days;
                var weeks = diff / 7;
                if (weeks < 0) return planStart;
                return planStart.AddDays(weeks * 7);
            }
            return planStart;
        }
    }
}

