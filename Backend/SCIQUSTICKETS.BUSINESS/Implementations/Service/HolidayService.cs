using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.HolidayRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.HolidayResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.HolidayDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class HolidayService : IHolidayService
	{
		private readonly AppDbContext _context;

		public HolidayService(AppDbContext context)
		{
			_context = context;
		}

		// ============================================================
		// ADMIN: CALENDAR CRUD
		// ============================================================

		public async Task<IEnumerable<HolidayResponse>> GetAllAsync()
		{
			return await _context.Holidays
				.AsNoTracking()
				.Where(h => !h.IsDeleted)
				.OrderBy(h => h.Date)
				.Select(h => ToResponse(h))
				.ToListAsync();
		}

		public async Task<HolidayResponse?> GetByIdAsync(Guid id)
		{
			var holiday = await _context.Holidays
				.AsNoTracking()
				.FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

			return holiday == null ? null : ToResponse(holiday);
		}

		public async Task<HolidayResponse> CreateAsync(CreateHolidayRequest request)
		{
			var now = TimeHelper.GetIndianTime();

			var holiday = new Holiday
			{
				Name = request.Name,
				Date = request.Date.Date,
				IsRecurringYearly = request.IsRecurringYearly,
				Description = request.Description,
				CreatedDate = now
			};

			await _context.Holidays.AddAsync(holiday);
			await _context.SaveChangesAsync();

			// Auto-create Pending confirmations for every active employee.
			// Pending == treated as Unavailable by AssignmentEngine until the
			// employee actively confirms they will be working.
			var activeEmployeeIds = await _context.Employees
				.AsNoTracking()
				.Where(e => !e.IsDeleted)
				.Select(e => e.Id)
				.ToListAsync();

			var confirmations = activeEmployeeIds.Select(empId => new HolidayConfirmation
			{
				HolidayId = holiday.Id,
				EmployeeId = empId,
				Status = "Pending",
				CreatedDate = now
			});

			await _context.HolidayConfirmations.AddRangeAsync(confirmations);
			await _context.SaveChangesAsync();

			return ToResponse(holiday);
		}

		public async Task<HolidayResponse> UpdateAsync(Guid id, UpdateHolidayRequest request)
		{
			var holiday = await _context.Holidays
				.FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

			if (holiday == null)
				throw new KeyNotFoundException("Holiday not found.");

			holiday.Name = request.Name;
			holiday.Date = request.Date.Date;
			holiday.IsRecurringYearly = request.IsRecurringYearly;
			holiday.Description = request.Description;

			await _context.SaveChangesAsync();

			return ToResponse(holiday);
		}

		public async Task<bool> SoftDeleteAsync(Guid id)
		{
			var holiday = await _context.Holidays
				.FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

			if (holiday == null)
				return false;

			holiday.IsDeleted = true;
			await _context.SaveChangesAsync();
			return true;
		}

		// ============================================================
		// EMPLOYEE: CONFIRMATIONS
		// ============================================================

		public async Task<IEnumerable<HolidayConfirmationResponse>> GetConfirmationsForEmployeeAsync(string employeeId)
		{
			return await _context.HolidayConfirmations
				.AsNoTracking()
				.Include(c => c.Holiday)
				.Where(c => c.EmployeeId == employeeId &&
							!c.IsDeleted &&
							!c.Holiday.IsDeleted)
				.OrderBy(c => c.Holiday.Date)
				.Select(c => new HolidayConfirmationResponse
				{
					Id = c.Id,
					HolidayId = c.HolidayId,
					HolidayName = c.Holiday.Name,
					HolidayDate = c.Holiday.Date,
					EmployeeId = c.EmployeeId,
					Status = c.Status,
					RespondedDate = c.RespondedDate
				})
				.ToListAsync();
		}

		public async Task<HolidayConfirmationResponse> ConfirmAsync(
			string employeeId,
			Guid holidayId,
			ConfirmHolidayRequest request)
		{
			var holiday = await _context.Holidays
				.AsNoTracking()
				.FirstOrDefaultAsync(h => h.Id == holidayId && !h.IsDeleted);

			if (holiday == null)
				throw new KeyNotFoundException("Holiday not found.");

			var confirmation = await _context.HolidayConfirmations
				.FirstOrDefaultAsync(c =>
					c.HolidayId == holidayId &&
					c.EmployeeId == employeeId &&
					!c.IsDeleted);

			var now = TimeHelper.GetIndianTime();

			// Covers employees added after the holiday was created
			// (no confirmation row was auto-seeded for them).
			if (confirmation == null)
			{
				confirmation = new HolidayConfirmation
				{
					HolidayId = holidayId,
					EmployeeId = employeeId,
					CreatedDate = now
				};
				await _context.HolidayConfirmations.AddAsync(confirmation);
			}

			confirmation.Status = request.IsAvailable ? "Available" : "Unavailable";
			confirmation.RespondedDate = now;

			await _context.SaveChangesAsync();

			return new HolidayConfirmationResponse
			{
				Id = confirmation.Id,
				HolidayId = holiday.Id,
				HolidayName = holiday.Name,
				HolidayDate = holiday.Date,
				EmployeeId = employeeId,
				Status = confirmation.Status,
				RespondedDate = confirmation.RespondedDate
			};
		}

		private static HolidayResponse ToResponse(Holiday h) => new()
		{
			Id = h.Id,
			Name = h.Name,
			Date = h.Date,
			IsRecurringYearly = h.IsRecurringYearly,
			Description = h.Description,
			IsDeleted = h.IsDeleted,
			CreatedDate = h.CreatedDate
		};
	}
}