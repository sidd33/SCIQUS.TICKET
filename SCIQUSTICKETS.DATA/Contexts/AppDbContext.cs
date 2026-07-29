using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.COMMON.Constants;
using SCIQUSTICKETS.DATA.DomainModels;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;
using SCIQUSTICKETS.DATA.DomainModels.DepartmentsDATA;

// ════════════════════════════════════════════════════════════════════════════
//  HOW TO ADD YOUR TEAM'S ENTITIES TO THIS FILE
//  ─────────────────────────────────────────────
//  1. Add your DbSet<YourEntity> in the "── DbSets ──" section below,
//     inside your team's labeled block.
//  2. Add your relationship configs and seed data in OnModelCreating,
//     inside your team's labeled block.
//  3. DO NOT touch or modify any other team's section.
//  4. Run: dotnet ef migrations add YourMigrationName --project SCIQUSTICKETS.DATA
//             --startup-project SCIQUSTICKETS.WebAPI
// ════════════════════════════════════════════════════════════════════════════

namespace SCIQUSTICKETS.DATA.Contexts
{
	public class AppDbContext : IdentityDbContext<ApplicationUser, UserRole, string>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		// ════════════════════════════════════════════════════════════════
		//  DbSets — ADD YOUR TEAM'S DbSets IN YOUR SECTION ONLY
		// ════════════════════════════════════════════════════════════════

		// ── [TEAM: AUTH/IDENTITY] ─────────────────────────────────────
		public DbSet<RefreshToken> RefreshTokens { get; set; }
		public DbSet<Policy> Policies { get; set; }
		public DbSet<RolePolicy> RolePolicies { get; set; }
		public DbSet<SpecializedPolicy> SpecializedPolicies { get; set; }
		// ── [END: AUTH/IDENTITY] ──────────────────────────────────────

		// ── [TEAM: EMPLOYEE] ──────────────────────────────────────────
		public DbSet<Employee> Employees { get; set; }
		public DbSet<Department> Departments { get; set; }
		public DbSet<Grade> Grades { get; set; }
		// ── [END: EMPLOYEE] ───────────────────────────────────────────

		// ── [TEAM: ACCOUNTS / CRM] ────────────────────────────────────
		public DbSet<Account> Accounts { get; set; }
		public DbSet<AccountTypes> AccountTypes { get; set; }
		public DbSet<IndustryTypes> IndustryTypes { get; set; }
		public DbSet<Region> Regions { get; set; }
		public DbSet<Currency> Currencies { get; set; }
		public DbSet<AccountContacts> AccountContacts { get; set; }
		public DbSet<AccountAddress> AccountAddresses { get; set; }
		public DbSet<AccountAddressType> AccountAddressTypes { get; set; }
		// ── [END: ACCOUNTS / CRM] ─────────────────────────────────────

		// ── [TEAM: TICKETS] ───────────────────────────────────────────
		// public DbSet<Ticket> Tickets { get; set; }
		// ── [END: TICKETS] ────────────────────────────────────────────


		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			// ════════════════════════════════════════════════════════════
			//  OnModelCreating — ADD YOUR CONFIG IN YOUR SECTION ONLY
			// ════════════════════════════════════════════════════════════

			// ── [TEAM: AUTH/IDENTITY] — Relationships ─────────────────

			builder.Entity<RolePolicy>()
				.HasKey(rp => new { rp.RoleId, rp.PolicyId });

			builder.Entity<RolePolicy>()
				.HasOne(rp => rp.Role)
				.WithMany(r => r.RolePolicies)
				.HasForeignKey(rp => rp.RoleId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<RolePolicy>()
				.HasOne(rp => rp.Policy)
				.WithMany(p => p.RolePolicies)
				.HasForeignKey(rp => rp.PolicyId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<SpecializedPolicy>()
				.HasKey(sp => new { sp.UserId, sp.PolicyId });

			builder.Entity<SpecializedPolicy>()
				.HasOne(sp => sp.User)
				.WithMany(u => u.SpecializedPolicies)
				.HasForeignKey(sp => sp.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<SpecializedPolicy>()
				.HasOne(sp => sp.Policy)
				.WithMany(p => p.SpecializedPolicies)
				.HasForeignKey(sp => sp.PolicyId)
				.OnDelete(DeleteBehavior.Cascade);

			// ── [SIDD: AUTH/IDENTITY] — Seed Data ────────────────────

			builder.Entity<UserRole>().HasData(
				new UserRole
				{
					Id = SEED.AdminRoleId,
					Name = SEED.AdminRole,
					NormalizedName = SEED.AdminRole.ToUpper(),
					CreatedDate = SEED.SeedDate,
					LastModifiedDate = SEED.SeedDate,
					IsDeleted = false,
					ConcurrencyStamp = SEED.AdminRoleConcurrencyStamp
				}
			);

			builder.Entity<ApplicationUser>().HasData(
				new ApplicationUser
				{
					Id = SEED.AdminUserId,
					UserName = SEED.AdminEmailId,
					NormalizedUserName = SEED.AdminEmailId.ToUpper(),
					Email = SEED.AdminEmailId,
					NormalizedEmail = SEED.AdminEmailId.ToUpper(),
					EmailConfirmed = true,
					PasswordHash = SEED.AdminPasswordHash,
					SecurityStamp = SEED.AdminSecurityStamp,
					ConcurrencyStamp = SEED.AdminConcurrencyStamp,
					CreatedDate = SEED.SeedDate,
					LastModifiedDate = SEED.SeedDate,
					Status = true,
					HasLoginAccess = true,
				}
			);

			builder.Entity<IdentityUserRole<string>>().HasData(
				new IdentityUserRole<string>
				{
					RoleId = SEED.AdminRoleId,
					UserId = SEED.AdminUserId
				}
			);

			// ── [END: AUTH/IDENTITY] ──────────────────────────────────


			// ── [TEAM: EMPLOYEE] — Relationships & Seed ───────────────

			builder.Entity<Employee>()
				.HasOne(e => e.ReportsToUser)
				.WithMany()
				.HasForeignKey(e => e.ReportsTo)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Employee>()
				.HasOne(e => e.Department)
				.WithMany(d => d.Employees)
				.HasForeignKey(e => e.DepartmentId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Department>()
				.HasOne(d => d.DepartmentHead)
				.WithMany()
				.HasForeignKey(d => d.DepartmentHeadId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Employee>()
				.HasOne(e => e.Grade)
				.WithMany(g => g.Employees)
				.HasForeignKey(e => e.GradeId)
				.OnDelete(DeleteBehavior.Restrict);

			// TODO: Employee/Department seed data — uncomment and adapt once
			// an admin ApplicationUser/Employee pairing convention is confirmed
			// with the Auth team (Employee.Id must equal ApplicationUser.Id).
			//
			// builder.Entity<Department>().HasData(
			//     new Department
			//     {
			//         DepartmentId = Guid.Parse(SEED.DeptId),
			//         Name = "Administration",
			//         IsDeleted = false,
			//         CreatedDate = SEED.SeedDate,
			//         LastModifiedDate = SEED.SeedDate
			//     }
			// );
			//
			// builder.Entity<Employee>().HasData(
			//     new Employee
			//     {
			//         Id = SEED.AdminUserId,
			//         Name = SEED.AdminName,
			//         Email = SEED.AdminEmailId,
			//         RegisteredMobileNumber = SEED.AdminRegisteredMobileNumber,
			//         SecondMobileNumber = SEED.AdminSecondMobileNumber,
			//         EmployeeId = SEED.AdminEmployeeId,
			//         Designation = SEED.AdminRole,
			//         ProfileImageUrl = SEED.AdminProfileImageUrl,
			//         AutoGenrateId = $"{SEED.EmployeeIdFormat}{1}",
			//         ReportsTo = null,
			//         DepartmentId = Guid.Parse(SEED.DeptId),
			//         CreatedDate = SEED.SeedDate,
			//         LastUpdatedDate = SEED.SeedDate,
			//     }
			// );

			// ── [END: EMPLOYEE] ───────────────────────────────────────

			// ── [TEAM: ACCOUNTS / CRM] — Relationships ────────────────
			builder.Entity<Account>()
				.HasMany(a => a.Contacts)
				.WithOne(c => c.Account)
				.HasForeignKey(c => c.AccountId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Account>()
				.HasOne(a => a.ReferralAccountContacts)
				.WithMany()
				.HasForeignKey(a => a.ReferralAccountContactsId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Account>()
				.HasMany(a => a.Addresses)
				.WithOne(ad => ad.Account)
				.HasForeignKey(ad => ad.AccountId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<AccountAddress>()
				.HasMany(ad => ad.AddressTypes)
				.WithOne(at => at.AccountAddress)
				.HasForeignKey(at => at.AccountAddressId)
				.OnDelete(DeleteBehavior.Cascade);
			// ── [END: ACCOUNTS / CRM] ─────────────────────────────────

			// ── [TEAM: TICKETS] — Relationships & Seed ────────────────
			// Add your Ticket config here.
			// ── [END: TICKETS] ────────────────────────────────────────
		}
	}
}