using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.COMMON.Constants;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;
using SCIQUSTICKETS.DATA.DomainModels.DepartmentsDATA;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.DomainModels.SupportPlanDATA;

namespace SCIQUSTICKETS.WebAPI
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            {
                await context.Database.MigrateAsync();
            }

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<UserRole>>();

            // 1. Ensure Roles Exist
            string[] roles = { "Admin", "SuperAdmin", "Employee", "Customer", "DepartmentHead" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new UserRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    });
                }
            }

            // 2. Ensure 5 Departments Exist
            var dept1Id = Guid.Parse("d1e2f3a4-b5c6-7890-abcd-123456789001");
            var dept2Id = Guid.Parse("d1e2f3a4-b5c6-7890-abcd-123456789002");
            var dept3Id = Guid.Parse("d1e2f3a4-b5c6-7890-abcd-123456789003");
            var dept4Id = Guid.Parse("d1e2f3a4-b5c6-7890-abcd-123456789004");
            var dept5Id = Guid.Parse("d1e2f3a4-b5c6-7890-abcd-123456789005");

            var dept1 = await context.Departments.FindAsync(dept1Id);
            if (dept1 == null)
            {
                dept1 = new Department { DepartmentId = dept1Id, Name = "IT Support & Infrastructure", TicketAutoAssignMethod = "Auto_assignment_custom", CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow, IsDeleted = false };
                context.Departments.Add(dept1);
            }

            var dept2 = await context.Departments.FindAsync(dept2Id);
            if (dept2 == null)
            {
                dept2 = new Department { DepartmentId = dept2Id, Name = "Customer Success & Account Management", TicketAutoAssignMethod = "RoundRobin", CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow, IsDeleted = false };
                context.Departments.Add(dept2);
            }

            var dept3 = await context.Departments.FindAsync(dept3Id);
            if (dept3 == null)
            {
                dept3 = new Department { DepartmentId = dept3Id, Name = "Product Engineering & Software Development", TicketAutoAssignMethod = "LoadBalanced", CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow, IsDeleted = false };
                context.Departments.Add(dept3);
            }

            var dept4 = await context.Departments.FindAsync(dept4Id);
            if (dept4 == null)
            {
                dept4 = new Department { DepartmentId = dept4Id, Name = "Finance & Billing Operations", TicketAutoAssignMethod = "RoundRobin", CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow, IsDeleted = false };
                context.Departments.Add(dept4);
            }

            var dept5 = await context.Departments.FindAsync(dept5Id);
            if (dept5 == null)
            {
                dept5 = new Department { DepartmentId = dept5Id, Name = "HR & Corporate Operations", TicketAutoAssignMethod = "Auto_assignment_custom", CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow, IsDeleted = false };
                context.Departments.Add(dept5);
            }
            await context.SaveChangesAsync();

			// Ensure System User Exists
			// Ensure System User Exists
			var systemEmail = "system@sciqustickets.internal";

			var systemUser = await userManager.FindByEmailAsync(systemEmail);

			if (systemUser == null)
			{
                systemUser = new ApplicationUser
                {
                    Id = SEED.SystemActorUserId,
                    UserName = systemEmail,
                    Email = systemEmail,
                    EmailConfirmed = true,
                    Status = true,
                    HasLoginAccess = false,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

				var systemResult = await userManager.CreateAsync(
					systemUser,
					"SystemAccount#2026Secure!"
				);

				if (!systemResult.Succeeded)
				{
					throw new Exception(
						$"Failed to create system user: {string.Join(", ", systemResult.Errors.Select(e => e.Description))}"
					);
				}
			}
		
			// 3. Ensure Seed Admin User
			var adminEmail = "admin@sciqustickets.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Status = true,
                    HasLoginAccess = true,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(adminUser, "Admin@123");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                }
            }

            if (!context.Employees.Any(e => e.Id == adminUser.Id))
            {
				context.Employees.Add(new Employee
				{
					Id = adminUser.Id,
					Name = "Super Admin",
					Email = adminEmail,
					AutoGenrateId = "ADM-0001",
					EmployeeId = "ADM-0001",
					Designation = "System Super Administrator",
					DepartmentId = dept1Id,   // ADD THIS
					CreatedDate = DateTime.UtcNow,
					LastUpdatedDate = DateTime.UtcNow,
					IsDeleted = false
				}); ;
                await context.SaveChangesAsync();
            }

            // 4. Seed Employees across 5 Departments
            var employeeSeedData = new[]
            {
                new { Email = "alex.turner@sciqustickets.com", Name = "Alex Turner", DeptId = dept1Id, EmpCode = "EMP-1001", IsHead = true },
                new { Email = "sarah.jenkins@sciqustickets.com", Name = "Sarah Jenkins", DeptId = dept1Id, EmpCode = "EMP-1002", IsHead = false },
                new { Email = "michael.brown@sciqustickets.com", Name = "Michael Brown", DeptId = dept1Id, EmpCode = "EMP-1003", IsHead = false },
                new { Email = "david.wilson@sciqustickets.com", Name = "David Wilson", DeptId = dept1Id, EmpCode = "EMP-1004", IsHead = false },
                new { Email = "emily.davis@sciqustickets.com", Name = "Emily Davis", DeptId = dept1Id, EmpCode = "EMP-1005", IsHead = false },

                new { Email = "jessica.white@sciqustickets.com", Name = "Jessica White", DeptId = dept2Id, EmpCode = "EMP-2001", IsHead = true },
                new { Email = "daniel.harris@sciqustickets.com", Name = "Daniel Harris", DeptId = dept2Id, EmpCode = "EMP-2002", IsHead = false },
                new { Email = "amanda.clark@sciqustickets.com", Name = "Amanda Clark", DeptId = dept2Id, EmpCode = "EMP-2003", IsHead = false },

                new { Email = "ryan.young@sciqustickets.com", Name = "Ryan Young", DeptId = dept3Id, EmpCode = "EMP-3001", IsHead = true },
                new { Email = "hannah.king@sciqustickets.com", Name = "Hannah King", DeptId = dept3Id, EmpCode = "EMP-3002", IsHead = false },
                new { Email = "andrew.wright@sciqustickets.com", Name = "Andrew Wright", DeptId = dept3Id, EmpCode = "EMP-3003", IsHead = false },

                new { Email = "katherine.billing@sciqustickets.com", Name = "Katherine Miller", DeptId = dept4Id, EmpCode = "EMP-4001", IsHead = true },
                new { Email = "brian.finance@sciqustickets.com", Name = "Brian Taylor", DeptId = dept4Id, EmpCode = "EMP-4002", IsHead = false },

                new { Email = "laura.hr@sciqustickets.com", Name = "Laura Anderson", DeptId = dept5Id, EmpCode = "EMP-5001", IsHead = true },
                new { Email = "kevin.hr@sciqustickets.com", Name = "Kevin Thomas", DeptId = dept5Id, EmpCode = "EMP-5002", IsHead = false }
            };

            foreach (var empData in employeeSeedData)
            {
                var empUser = await userManager.FindByEmailAsync(empData.Email);
                if (empUser == null)
                {
                    empUser = new ApplicationUser
                    {
                        UserName = empData.Email,
                        Email = empData.Email,
                        EmailConfirmed = true,
                        Status = true,
                        HasLoginAccess = true,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    };
                    var result = await userManager.CreateAsync(empUser, "Employee@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(empUser, "Employee");
                        if (empData.IsHead)
                        {
                            await userManager.AddToRoleAsync(empUser, "DepartmentHead");
                        }
                    }
                }

                if (empUser != null && !context.Employees.Any(e => e.Id == empUser.Id))
                {
                    context.Employees.Add(new Employee
                    {
                        Id = empUser.Id,
                        Name = empData.Name,
                        Email = empData.Email,
                        AutoGenrateId = empData.EmpCode,
                        EmployeeId = empData.EmpCode,
                        DepartmentId = empData.DeptId,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    });
                    await context.SaveChangesAsync();

                    if (empData.IsHead)
                    {
                        var targetDept = await context.Departments.FindAsync(empData.DeptId);
                        if (targetDept != null)
                        {
                            targetDept.DepartmentHeadId = empUser.Id;
                            context.Departments.Update(targetDept);
                            await context.SaveChangesAsync();
                        }
                    }
                }
            }

            // 5. Seed 10 Customer Companies with CRM Details
            var customerSeedData = new[]
            {
                new { Email = "customer.acme@acmecorp.com", Company = "Acme Corporation", Phone = "+1 (555) 019-2831", AccCode = "ACC-1001" },
                new { Email = "customer.apex@apextech.com", Company = "Apex Technologies", Phone = "+1 (555) 019-4820", AccCode = "ACC-1002" },
                new { Email = "customer.nexus@nexusglobal.com", Company = "Nexus Global Solutions", Phone = "+1 (555) 019-3918", AccCode = "ACC-1003" },
                new { Email = "customer.omni@omnicorp.io", Company = "OmniCorp Media", Phone = "+1 (555) 019-5729", AccCode = "ACC-1004" },
                new { Email = "customer.horizon@horizoninc.com", Company = "Horizon Innovations", Phone = "+1 (555) 019-8201", AccCode = "ACC-1005" },
                new { Email = "customer.vortex@vortextools.com", Company = "Vortex Analytics", Phone = "+1 (555) 019-1948", AccCode = "ACC-1006" },
                new { Email = "customer.synergy@synergyhealth.org", Company = "Synergy Health", Phone = "+1 (555) 019-6382", AccCode = "ACC-1007" },
                new { Email = "customer.quantum@quantumdata.com", Company = "Quantum Data Systems", Phone = "+1 (555) 019-7410", AccCode = "ACC-1008" },
                new { Email = "customer.stellar@stellarmedia.net", Company = "Stellar Networks", Phone = "+1 (555) 019-9283", AccCode = "ACC-1009" },
                new { Email = "customer.pinnacle@pinnaclefinance.com", Company = "Pinnacle Financial", Phone = "+1 (555) 019-2049", AccCode = "ACC-1010" },
				new { Email = "customer.pinnacle@pinnaclefinance.com", Company = "Pinnacle Financial", Phone = "+1 (555) 019-2049", AccCode = "ACC-1010" },
                new { Email = "customer.orbit@orbitlogistics.com", Company = "Orbit Logistics", Phone = "+1 (555) 019-3157", AccCode = "ACC-1011" },
                new { Email = "customer.vertex@vertexsystems.com", Company = "Vertex Systems", Phone = "+1 (555) 019-4628", AccCode = "ACC-1012" },
                new { Email = "customer.bluewave@bluewaveconsulting.com", Company = "BlueWave Consulting", Phone = "+1 (555) 019-5834", AccCode = "ACC-1013" },
                new { Email = "customer.novatek@novatekindustries.com", Company = "NovaTek Industries", Phone = "+1 (555) 019-6742", AccCode = "ACC-1014" },
                new { Email = "customer.crescent@crescentdigital.com", Company = "Crescent Digital Solutions", Phone = "+1 (555) 019-7951", AccCode = "ACC-1015" }
			    };

            foreach (var custData in customerSeedData)
            {
                var custUser = await userManager.FindByEmailAsync(custData.Email);
                if (custUser == null)
                {
                    custUser = new ApplicationUser
                    {
                        UserName = custData.Email,
                        Email = custData.Email,
                        EmailConfirmed = true,
                        Status = true,
                        HasLoginAccess = true,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    };
                    var result = await userManager.CreateAsync(custUser, "Customer@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(custUser, "Customer");
                    }
                }

                if (custUser != null && !context.Accounts.Any(a => a.AccountId == custUser.Id || a.Email == custData.Email))
                {
                    context.Accounts.Add(new Account
                    {
                        AccountId = custUser.Id,
                        AccountName = custData.Company,
                        Email = custData.Email,
                        RegisteredMobileNumber = custData.Phone,
                        AutoGenerateAccountId = custData.AccCode,
                        Status = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow
                    });
                    await context.SaveChangesAsync();
                }
            }

            // Seed test Account Contact for WhatsApp
            var acmeAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Email == "customer.acme@acmecorp.com");
            if (acmeAccount != null && !context.AccountContacts.Any(c => c.MobileNumber == "+16315551181"))
            {
                context.AccountContacts.Add(new AccountContacts
                {
                    AccountContactsId = Guid.NewGuid(),
                    AccountId = acmeAccount.AccountId,
                    PersonName = "Test User",
                    Email = "test@acmecorp.com",
                    MobileNumber = "+16315551181",
                    IsDeleted = false,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

            // 6. Ensure Ticket Statuses Exist
            var openStatusId = Guid.Parse("10000000-0000-0000-0000-000000000001");
            var inProgressStatusId = Guid.Parse("10000000-0000-0000-0000-000000000002");
            var pendingClosureStatusId = Guid.Parse("10000000-0000-0000-0000-000000000006");
            var closedStatusId = Guid.Parse("10000000-0000-0000-0000-000000000005");
            var reopenedStatusId = Guid.Parse("10000000-0000-0000-0000-000000000007");

            if (!context.TicketStatuses.Any())
            {
                context.TicketStatuses.AddRange(
                    new TicketStatus { TicketStatusId = openStatusId, Name = "Open", Description = "New ticket created", IsClosed = false, Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow },
                    new TicketStatus { TicketStatusId = inProgressStatusId, Name = "In Progress", Description = "Ticket is being worked on", IsClosed = false, Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow },
                    new TicketStatus { TicketStatusId = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "Pending", Description = "Waiting for information", IsClosed = false, Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow },
                    new TicketStatus { TicketStatusId = Guid.Parse("10000000-0000-0000-0000-000000000004"), Name = "Resolved", Description = "Solution provided", IsClosed = false, Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow },
                    new TicketStatus { TicketStatusId = closedStatusId, Name = "Closed", Description = "Ticket closed successfully", IsClosed = true, Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow },
                    new TicketStatus { TicketStatusId = pendingClosureStatusId, Name = "PendingClosure", Description = "Resolved, awaiting 24h customer confirmation", IsClosed = false, Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow },
                    new TicketStatus { TicketStatusId = reopenedStatusId, Name = "Reopened", Description = "Ticket reopened after closure", IsClosed = false, Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }

            // 7. Ensure Priorities Exist (Min 36h SLA Floor)
            if (!context.TicketPriorities.Any())
            {
                context.TicketPriorities.AddRange(
                    new TicketPriority { TicketPriorityId = Guid.NewGuid(), Name = "Critical", Level = 4, SlaInHours = 36, ResponseSlaInHours = 1, Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow },
                    new TicketPriority { TicketPriorityId = Guid.NewGuid(), Name = "High", Level = 3, SlaInHours = 48, ResponseSlaInHours = 2, Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow },
                    new TicketPriority { TicketPriorityId = Guid.NewGuid(), Name = "Medium", Level = 2, SlaInHours = 72, ResponseSlaInHours = 4, Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow },
                    new TicketPriority { TicketPriorityId = Guid.NewGuid(), Name = "Low", Level = 1, SlaInHours = 96, ResponseSlaInHours = 8, Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }

            // 8. Ensure Business Impacts Exist
            if (!context.TicketBusinessTypeImpacts.Any())
            {
                context.TicketBusinessTypeImpacts.AddRange(
                    new TicketBusinessTypeImpact { TicketBusinessTypeImpactId = Guid.NewGuid(), Name = "Organization Wide / Critical Outage", Description = "Entire company or major department impacted", Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow },
                    new TicketBusinessTypeImpact { TicketBusinessTypeImpactId = Guid.NewGuid(), Name = "Multiple Users / High", Description = "Multiple staff members blocked", Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow },
                    new TicketBusinessTypeImpact { TicketBusinessTypeImpactId = Guid.NewGuid(), Name = "Single User / Medium", Description = "Single staff member impacted with workaround", Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow },
                    new TicketBusinessTypeImpact { TicketBusinessTypeImpactId = Guid.NewGuid(), Name = "Minor / General Query", Description = "Minor question or cosmetic issue", Status = true, CreatedDate = DateTime.UtcNow, LastUpdatedDate = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }

			// 9. Ensure Types & Sub-Types Exist
			// 9. Ensure Types & Sub-Types Exist

			var hardwareType = await context.TicketTypes
				.FirstOrDefaultAsync(t => t.Name == "Hardware & Devices" && !t.IsDeleted);

			if (hardwareType == null)
			{
				hardwareType = new TicketType
				{
					TicketTypeId = Guid.NewGuid(),
					Name = "Hardware & Devices",
					Description = "Laptop, Monitor, Printer, Network hardware",
					Status = true,
					IsDeleted = false,
					CreatedDate = DateTime.UtcNow,
					LastUpdatedDate = DateTime.UtcNow
				};

				context.TicketTypes.Add(hardwareType);
			}

			var softwareType = await context.TicketTypes
				.FirstOrDefaultAsync(t => t.Name == "Software & Apps" && !t.IsDeleted);

			if (softwareType == null)
			{
				softwareType = new TicketType
				{
					TicketTypeId = Guid.NewGuid(),
					Name = "Software & Apps",
					Description = "OS, Office 365, VPN, ERP, Email access",
					Status = true,
					IsDeleted = false,
					CreatedDate = DateTime.UtcNow,
					LastUpdatedDate = DateTime.UtcNow
				};

				context.TicketTypes.Add(softwareType);
			}

			var otherType = await context.TicketTypes
				.FirstOrDefaultAsync(t => t.Name == "Other" && !t.IsDeleted);

			if (otherType == null)
			{
				otherType = new TicketType
				{
					TicketTypeId = Guid.NewGuid(),
					Name = "Other",
					Description = "Issue does not match any of the available ticket types",
					Status = true,
					IsDeleted = false,
					CreatedDate = DateTime.UtcNow,
					LastUpdatedDate = DateTime.UtcNow
				};

				context.TicketTypes.Add(otherType);
			}

			await context.SaveChangesAsync();

			// 10. Ensure Ticket ID Store Initialized
			// 10. Ensure Ticket Sub-Types Exist

			var subTypeSeedData = new[]
			{
    // Hardware & Devices
    new
	{
		Name = "Laptop Issue",
		Description = "Laptop hardware or device related issue",
		TicketTypeId = hardwareType.TicketTypeId,
		DepartmentId = dept1Id
	},
	new
	{
		Name = "Printer Issue",
		Description = "Printer or printing hardware related issue",
		TicketTypeId = hardwareType.TicketTypeId,
		DepartmentId = dept1Id
	},
	new
	{
		Name = "Network Device Issue",
		Description = "Router, switch or other network device issue",
		TicketTypeId = hardwareType.TicketTypeId,
		DepartmentId = dept1Id
	},

    // Software & Apps
    new
	{
		Name = "VPN Issue",
		Description = "VPN connection or authentication issue",
		TicketTypeId = softwareType.TicketTypeId,
		DepartmentId = dept1Id
	},
	new
	{
		Name = "Email / Outlook Issue",
		Description = "Email, Outlook or mailbox related issue",
		TicketTypeId = softwareType.TicketTypeId,
		DepartmentId = dept1Id
	},
	new
	{
		Name = "Application Access Issue",
		Description = "Access or permission issue for an application",
		TicketTypeId = softwareType.TicketTypeId,
		DepartmentId = dept1Id
	}
};

			foreach (var data in subTypeSeedData)
			{
				var existingSubType = await context.TicketSubTypes
					.FirstOrDefaultAsync(st =>
						st.TicketTypeId == data.TicketTypeId &&
						st.Name == data.Name &&
						!st.IsDeleted);

				if (existingSubType == null)
				{
					context.TicketSubTypes.Add(new TicketSubType
					{
						TicketSubTypeId = Guid.NewGuid(),
						Name = data.Name,
						Description = data.Description,
						TicketTypeId = data.TicketTypeId,
						DepartmentId = data.DepartmentId,

						// IMPORTANT
						RequiresAcceptance = true,

						Status = true,
						IsDeleted = false,
						CreatedDate = DateTime.UtcNow,
						LastUpdatedDate = DateTime.UtcNow
					});
				}
			}

			await context.SaveChangesAsync();

			// 11. Seed Dummy Tickets across all states
			if (!context.Tickets.Any() || context.Tickets.Any(t => t.TicketTypeId == Guid.Empty))
            {
                var invalidTickets = context.Tickets.Where(t => t.TicketTypeId == Guid.Empty).ToList();
                if (invalidTickets.Any())
                {
                    context.Tickets.RemoveRange(invalidTickets);
                    await context.SaveChangesAsync();
                }

                if (!context.Tickets.Any())
                {
                    var priorityObj = context.TicketPriorities.FirstOrDefault();
                    var impactObj = context.TicketBusinessTypeImpacts.FirstOrDefault();
                    var subTypeObj = context.TicketSubTypes.FirstOrDefault();
                    var firstEmp = context.Employees.FirstOrDefault(e => e.DepartmentId == dept1Id);
                    var acmeCustomer = context.Accounts.FirstOrDefault();

                    if (priorityObj != null && impactObj != null && subTypeObj != null)
                    {
                        var creatorId = adminUser.Id;

                        var dummyTickets = new[]
                        {
                            new Ticket
                            {
                                TicketId = Guid.NewGuid(),
                                TicketNumber = "TKT-000101",
                                Title = "VPN Gateway Disconnection on Corporate Laptops",
                                Description = "Users reporting constant disconnects every 15 minutes when connected via Cisco AnyConnect VPN.",
                                StatusId = inProgressStatusId,
                                DepartmentId = dept1Id,
                                PriorityId = priorityObj.TicketPriorityId,
                                BusinessImpactId = impactObj.TicketBusinessTypeImpactId,
                                TicketTypeId = subTypeObj.TicketTypeId,
                                TicketSubTypeId = subTypeObj.TicketSubTypeId,
                                CreatedByUserId = creatorId,
                                AssignedToUserId = firstEmp?.Id,
                                AccountId = acmeCustomer?.AccountId,
                                SourceType = "WhatsApp",
                                IsOpen = true,
                                SlaDueDate = DateTime.UtcNow.AddHours(28),
                                CreatedDate = DateTime.UtcNow.AddHours(-8),
                                LastUpdatedDate = DateTime.UtcNow
                            },
                            new Ticket
                            {
                                TicketId = Guid.NewGuid(),
                                TicketNumber = "TKT-000102",
                                Title = "Invoice PDF Generation Failure in ERP Billing",
                                Description = "Generating monthly customer billing PDFs throws HTTP 500 Internal Error.",
                                StatusId = pendingClosureStatusId,
                                DepartmentId = dept4Id,
                                PriorityId = priorityObj.TicketPriorityId,
                                BusinessImpactId = impactObj.TicketBusinessTypeImpactId,
                                TicketTypeId = subTypeObj.TicketTypeId,
                                TicketSubTypeId = subTypeObj.TicketSubTypeId,
                                CreatedByUserId = creatorId,
                                AssignedToUserId = firstEmp?.Id,
                                AccountId = acmeCustomer?.AccountId,
                                SourceType = "Portal",
                                IsOpen = true,
                                PendingClosureDate = DateTime.UtcNow.AddHours(-4),
                                SlaDueDate = DateTime.UtcNow.AddHours(40),
                                CreatedDate = DateTime.UtcNow.AddHours(-18),
                                LastUpdatedDate = DateTime.UtcNow
                            },
                            new Ticket
                            {
                                TicketId = Guid.NewGuid(),
                                TicketNumber = "TKT-000103",
                                Title = "Request Reassignment: Database Connection Pool Exhausted",
                                Description = "Employee requested reassignment to Senior DBA team due to heavy connection query lockups.",
                                StatusId = openStatusId,
                                DepartmentId = dept3Id,
                                PriorityId = priorityObj.TicketPriorityId,
                                BusinessImpactId = impactObj.TicketBusinessTypeImpactId,
                                TicketTypeId = subTypeObj.TicketTypeId,
                                TicketSubTypeId = subTypeObj.TicketSubTypeId,
                                CreatedByUserId = creatorId,
                                AccountId = acmeCustomer?.AccountId,
                                SourceType = "Agent",
                                IsOpen = true,
                                SlaDueDate = DateTime.UtcNow.AddHours(12),
                                CreatedDate = DateTime.UtcNow.AddHours(-24),
                                LastUpdatedDate = DateTime.UtcNow
                            },
                            new Ticket
                            {
                                TicketId = Guid.NewGuid(),
                                TicketNumber = "TKT-000104",
                                Title = "SLA Breached: Critical Payment Gateway Timeout",
                                Description = "Payment processing endpoint failing to respond within SLA window.",
                                StatusId = inProgressStatusId,
                                DepartmentId = dept3Id,
                                PriorityId = priorityObj.TicketPriorityId,
                                BusinessImpactId = impactObj.TicketBusinessTypeImpactId,
                                TicketTypeId = subTypeObj.TicketTypeId,
                                TicketSubTypeId = subTypeObj.TicketSubTypeId,
                                CreatedByUserId = creatorId,
                                AssignedToUserId = firstEmp?.Id,
                                AccountId = acmeCustomer?.AccountId,
                                SourceType = "Email",
                                IsOpen = true,
                                IsSlaBreached = true,
                                SlaDueDate = DateTime.UtcNow.AddHours(-6),
                                CreatedDate = DateTime.UtcNow.AddHours(-48),
                                LastUpdatedDate = DateTime.UtcNow
                            }
                        };

                        context.Tickets.AddRange(dummyTickets);
                        await context.SaveChangesAsync();
                    }
                }
            }

            // 12. Seed WhatsApp Channel Config
            if (!context.WhatsAppChannelConfigs.Any())
            {
                var priorityObj = context.TicketPriorities.FirstOrDefault();
                var impactObj = context.TicketBusinessTypeImpacts.FirstOrDefault();
                var deptObj = context.Departments.FirstOrDefault();
                var typeObj = context.TicketTypes.FirstOrDefault();
                var subTypeObj = context.TicketSubTypes.FirstOrDefault();

				context.WhatsAppChannelConfigs.Add(new WhatsAppChannelConfig
				{
					WhatsAppChannelConfigId = Guid.NewGuid(),
					Provider = 0,

<<<<<<< HEAD
					BusinessPhoneNumberId = "1264781743381359",
=======
					BusinessPhoneNumberId = "15556638753",
>>>>>>> develop

					EncryptedApiToken = "...",

					WebhookVerifyToken = "sciqus_secret_token_123",

<<<<<<< HEAD
					AppSecret = "sciqus_app_secret_123",
=======
					AppSecret = "603e5be7252bb996d4c4c9f1ddde9f12",
>>>>>>> develop

					IsEnabled = true,
					AutoCreateEnabled = true,

					DefaultPriorityId = priorityObj?.TicketPriorityId ?? Guid.Empty,
					DefaultBusinessImpactId = impactObj?.TicketBusinessTypeImpactId ?? Guid.Empty,
					DefaultDepartmentId = deptObj?.DepartmentId ?? Guid.Empty,
					DefaultTicketTypeId = typeObj?.TicketTypeId ?? Guid.Empty,
					DefaultTicketSubTypeId = subTypeObj?.TicketSubTypeId ?? Guid.Empty
				});
				await context.SaveChangesAsync();
<<<<<<< HEAD
=======
            }

            // 13. Seed Support Plans and Contacts for Local Dev Testing
            if (!context.SupportPlans.Any())
            {
                var strictPlan = new SupportPlan
                {
                    SupportPlanId = Guid.NewGuid(),
                    Name = "Standard Plan - Strict Limit",
                    Description = "Only allows 5 tickets. Blocks any overages.",
                    TicketQuota = 5,
                    PeriodType = "Monthly",
                    ValidityDays = 30,
                    BlockWhenExhausted = true,
                    Status = true,
                    CreatedDate = DateTime.UtcNow
                };

                var overagePlan = new SupportPlan
                {
                    SupportPlanId = Guid.NewGuid(),
                    Name = "Premium Plan - Allows Overages",
                    Description = "Base quota of 10 tickets, but allows unlimited overages.",
                    TicketQuota = 10,
                    PeriodType = "Monthly",
                    ValidityDays = 30,
                    BlockWhenExhausted = false,
                    Status = true,
                    CreatedDate = DateTime.UtcNow
                };

                context.SupportPlans.AddRange(strictPlan, overagePlan);
                await context.SaveChangesAsync();

                // Fetch seeded accounts
                var acmeAccount = await context.Accounts.FirstOrDefaultAsync(a => a.AccountName.Contains("Acme"));
                var apexAccount = await context.Accounts.FirstOrDefaultAsync(a => a.AccountName.Contains("Apex"));

                if (acmeAccount != null)
                {
                    // Assign strict plan to Acme Corporation
                    context.AccountSupportPlans.Add(new AccountSupportPlan
                    {
                        AccountSupportPlanId = Guid.NewGuid(),
                        AccountId = acmeAccount.AccountId,
                        SupportPlanId = strictPlan.SupportPlanId,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(30),
                        Status = "Active",
                        CreatedDate = DateTime.UtcNow
                    });

                    // Add Siddhartha Swamy contact to Acme Corporation with phone +918888888888
                    if (!context.AccountContacts.Any(c => c.Email == "siddharthaswamy16@gmail.com"))
                    {
                        context.AccountContacts.Add(new AccountContacts
                        {
                            AccountContactsId = Guid.NewGuid(),
                            AccountId = acmeAccount.AccountId,
                            PersonName = "Siddhartha Swamy",
                            Email = "siddharthaswamy16@gmail.com",
                            MobileNumber = "+918888888888",
                            IsDeleted = false,
                            CreatedDate = DateTime.UtcNow,
                            LastUpdatedDate = DateTime.UtcNow
                        });
                    }
                }

                if (apexAccount != null)
                {
                    // Assign overage plan to Apex Technologies
                    context.AccountSupportPlans.Add(new AccountSupportPlan
                    {
                        AccountSupportPlanId = Guid.NewGuid(),
                        AccountId = apexAccount.AccountId,
                        SupportPlanId = overagePlan.SupportPlanId,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(30),
                        Status = "Active",
                        CreatedDate = DateTime.UtcNow
                    });

                    // Add Siddhartha Swamy contact to Apex Technologies with phone +919999999999
                    if (!context.AccountContacts.Any(c => c.MobileNumber == "+919999999999"))
                    {
                        context.AccountContacts.Add(new AccountContacts
                        {
                            AccountContactsId = Guid.NewGuid(),
                            AccountId = apexAccount.AccountId,
                            PersonName = "Siddhartha Swamy (WhatsApp)",
                            Email = "siddharthaswamy_wa@apextech.com",
                            MobileNumber = "+919999999999",
                            IsDeleted = false,
                            CreatedDate = DateTime.UtcNow,
                            LastUpdatedDate = DateTime.UtcNow
                        });
                    }
                }

                await context.SaveChangesAsync();
>>>>>>> develop
            }
        }
    }
}
