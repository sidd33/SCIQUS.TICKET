using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.COMMON.Constants;
using SCIQUSTICKETS.DATA.DomainModels;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;
using SCIQUSTICKETS.DATA.DomainModels.DepartmentsDATA;
using SCIQUSTICKETS.DATA.DomainModels.SupportPlanDATA;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCIQUSTICKETS.DATA.Contexts
{
	public class AppDbContext : IdentityDbContext<ApplicationUser, UserRole, string>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


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
		public DbSet<TicketType> TicketTypes { get; set; }
		public DbSet<TicketSubType> TicketSubTypes { get; set; }
		public DbSet<TicketPriority> TicketPriorities { get; set; }
		public DbSet<TicketBusinessTypeImpact> TicketBusinessTypeImpacts { get; set; }
		public DbSet<TicketAcceptance> TicketAcceptances { get; set; }
		// 🎫 [END: TICKET TRANSACTION] 

		// 🎫 [TEAM: SUPPORT PLANS] 
		public DbSet<SupportPlan> SupportPlans { get; set; }
		public DbSet<AccountSupportPlan> AccountSupportPlans { get; set; }
		public DbSet<SupportPlanConsumption> SupportPlanConsumptions { get; set; }
		// 🎫 [END: SUPPORT PLANS] 

		// ── [TEAM: TICKET TRANSACTION] ────────────────────────────────
		public DbSet<Ticket> Tickets { get; set; }
		public DbSet<TicketStatus> TicketStatuses { get; set; }
		public DbSet<TicketAssignment> TicketAssignments { get; set; }
		public DbSet<TicketComment> TicketComments { get; set; }
		public DbSet<TicketHistory> TicketHistories { get; set; }
		public DbSet<TicketAttachment> TicketAttachments { get; set; }
		public DbSet<TicketIDStore> TicketIDStores { get; set; }
		public DbSet<SlaConfiguration> SlaConfigurations { get; set; }
		public DbSet<TicketStateChangeHistory> TicketStateChangeHistories { get; set; }
		// ── [END: TICKET TRANSACTION] ─────────────────────────────────
		
		// ── [TEAM: NOTIFICATIONS] ─────────────────────────────────────
		public DbSet<Notification> Notifications { get; set; }
		public DbSet<NotificationUser> NotificationUsers { get; set; }
		public DbSet<NotificationData> NotificationData { get; set; }
		// ── [END: NOTIFICATIONS] ──────────────────────────────────────

		// ── [TEAM: CHANNELS] ──────────────────────────────────────────
		public DbSet<EmailTicketConfig> EmailTicketConfigs { get; set; }
		public DbSet<EmailInboxMessage> EmailInboxMessages { get; set; }
		
		public DbSet<WhatsAppChannelConfig> WhatsAppChannelConfigs { get; set; }
		public DbSet<WhatsAppInboxMessage> WhatsAppInboxMessages { get; set; }
		public DbSet<WhatsAppOutboundMessage> WhatsAppOutboundMessages { get; set; }
		// ── [END: CHANNELS] ───────────────────────────────────────────
		public DbSet<FaqArticle> FaqArticles { get; set; }
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

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
			builder.Entity<TicketSubType>()
				.HasOne(st => st.TicketType)
				.WithMany(tt => tt.TicketSubTypes)
				.HasForeignKey(st => st.TicketTypeId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<TicketSubType>()
				.HasOne(st => st.Department)
				.WithMany()
				.HasForeignKey(st => st.DepartmentId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<TicketSubType>()
				.HasOne(st => st.DefaultUser)
				.WithMany()
				.HasForeignKey(st => st.DefaultUserId)
				.OnDelete(DeleteBehavior.Restrict);
			// ── [END: TICKETS] ────────────────────────────────────────

			builder.Entity<Ticket>()
				.HasOne(t => t.Account)
				.WithMany()
				.HasForeignKey(t => t.AccountId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Ticket>()
				.HasOne(t => t.TicketType)
				.WithMany()
				.HasForeignKey(t => t.TicketTypeId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Ticket>()
				.HasOne(t => t.TicketSubType)
				.WithMany()
				.HasForeignKey(t => t.TicketSubTypeId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Ticket>()
				.HasOne(t => t.Priority)
				.WithMany()
				.HasForeignKey(t => t.PriorityId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Ticket>()
				.HasOne(t => t.BusinessImpact)
				.WithMany()
				.HasForeignKey(t => t.BusinessImpactId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Ticket>()
				.HasOne(t => t.Status)
				.WithMany(s => s.Tickets)
				.HasForeignKey(t => t.StatusId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Ticket>()
				.HasOne(t => t.RaisedByEmployee)
				.WithMany()
				.HasForeignKey(t => t.RaisedByEmployeeId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── [TEAM: NOTIFICATIONS] — Relationships ─────────────────
			builder.Entity<NotificationUser>()
				.HasOne(nu => nu.User)
				.WithMany()
				.HasForeignKey(nu => nu.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<NotificationData>()
				.HasOne(nd => nd.Notification)
				.WithOne(n => n.NotificationData)
				.HasForeignKey<NotificationData>(nd => nd.NotificationId)
				.OnDelete(DeleteBehavior.Cascade);

			// Add inside OnModelCreating, near the other Ticket relationship configs:
			builder.Entity<TicketAcceptance>()
				.HasOne(ta => ta.Ticket)
				.WithMany()
				.HasForeignKey(ta => ta.TicketId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<TicketAcceptance>()
				.HasOne(ta => ta.AssignedEmployee)
				.WithMany()
				.HasForeignKey(ta => ta.AssignedEmployeeId)
				.OnDelete(DeleteBehavior.Restrict);
			// ── [END: NOTIFICATIONS] ──────────────────────────────────

			// ── [TEAM: CHANNELS] — Relationships ──────────────────────
			builder.Entity<EmailInboxMessage>()
				.HasOne(m => m.CreatedTicket)
				.WithMany()
				.HasForeignKey(m => m.CreatedTicketId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<WhatsAppInboxMessage>()
				.HasOne(m => m.CreatedTicket)
				.WithMany()
				.HasForeignKey(m => m.CreatedTicketId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<WhatsAppOutboundMessage>()
				.HasOne(m => m.Ticket)
				.WithMany()
				.HasForeignKey(m => m.TicketId)
				.OnDelete(DeleteBehavior.Cascade);
			// ── [END: CHANNELS] ───────────────────────────────────────

			// ── [TEAM: TICKET STATUS SEED] ─────────────────────────────
			builder.Entity<TicketStatus>().HasData(
				new TicketStatus
				{
					TicketStatusId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
					Name = "Open",
					Description = "New ticket created",
					IsClosed = false,
					Status = true,
					IsDeleted = false,
					CreatedDate = SEED.SeedDate,
					LastUpdatedDate = SEED.SeedDate
				},
				new TicketStatus
				{
					TicketStatusId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
					Name = "In Progress",
					Description = "Ticket is being worked on",
					IsClosed = false,
					Status = true,
					IsDeleted = false,
					CreatedDate = SEED.SeedDate,
					LastUpdatedDate = SEED.SeedDate
				},
				new TicketStatus
				{
					TicketStatusId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
					Name = "Pending",
					Description = "Waiting for additional information",
					IsClosed = false,
					Status = true,
					IsDeleted = false,
					CreatedDate = SEED.SeedDate,
					LastUpdatedDate = SEED.SeedDate
				},
				new TicketStatus
				{
					TicketStatusId = Guid.Parse("10000000-0000-0000-0000-000000000004"),
					Name = "Resolved",
					Description = "Solution provided",
					IsClosed = false,
					Status = true,
					IsDeleted = false,
					CreatedDate = SEED.SeedDate,
					LastUpdatedDate = SEED.SeedDate
				},
				new TicketStatus
				{
					TicketStatusId = Guid.Parse("10000000-0000-0000-0000-000000000005"),
					Name = "Closed",
					Description = "Ticket closed successfully",
					IsClosed = true,
					Status = true,
					IsDeleted = false,
					CreatedDate = SEED.SeedDate,
					LastUpdatedDate = SEED.SeedDate
				},
				new TicketStatus
				{
					TicketStatusId = Guid.Parse("10000000-0000-0000-0000-000000000006"),
					Name = "PendingClosure",
					Description = "Resolved, awaiting customer confirmation",
					IsClosed = false,
					Status = true,
					IsDeleted = false,
					CreatedDate = SEED.SeedDate,
					LastUpdatedDate = SEED.SeedDate
				},
				new TicketStatus
				{
					TicketStatusId = Guid.Parse("10000000-0000-0000-0000-000000000007"),
					Name = "Reopened",
					Description = "Ticket reopened after closure",
					IsClosed = false,
					Status = true,
					IsDeleted = false,
					CreatedDate = SEED.SeedDate,
					LastUpdatedDate = SEED.SeedDate
				}
			);
			// ── [END: TICKET STATUS SEED] ───────────────────────────────

			builder.Entity<TicketIDStore>().HasData(
				new TicketIDStore
				{
					Id = 1,
					Prefix = "TKT",
					CurrentNumber = 0,
					LastUpdatedDate = SEED.SeedDate
				}
			);

			builder.Entity<Ticket>()
			.HasOne(t => t.ParentTicket)
			.WithMany()
			.HasForeignKey(t => t.ParentTicketId)
			.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<FaqArticle>()
			.HasOne(f => f.TicketType)
			.WithMany()
			.HasForeignKey(f => f.TicketTypeId)
			.OnDelete(DeleteBehavior.Restrict);

			// 🎫 [TEAM: SUPPORT PLANS] ── Relationships ────────────────
			builder.Entity<AccountSupportPlan>()
				.HasOne(asp => asp.Account)
				.WithMany()
				.HasForeignKey(asp => asp.AccountId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<AccountSupportPlan>()
				.HasOne(asp => asp.SupportPlan)
				.WithMany(sp => sp.AccountSupportPlans)
				.HasForeignKey(asp => asp.SupportPlanId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<SupportPlanConsumption>()
				.HasOne(spc => spc.AccountSupportPlan)
				.WithMany(asp => asp.Consumptions)
				.HasForeignKey(spc => spc.AccountSupportPlanId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<SupportPlanConsumption>()
				.HasOne(spc => spc.Ticket)
				.WithMany()
				.HasForeignKey(spc => spc.TicketId)
				.OnDelete(DeleteBehavior.Restrict);
			// 🎫 [END: SUPPORT PLANS] ──────────────────────────────────
		}
	}
}