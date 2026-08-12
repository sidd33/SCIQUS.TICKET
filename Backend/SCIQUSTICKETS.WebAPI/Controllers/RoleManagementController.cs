using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;
using System.Security.Claims;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleManagementController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleManagementController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Admin: Promote Employee to Department Head
        [HttpPost("promote-dept-head/{employeeId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> PromoteToDeptHead(string employeeId)
        {
            var emp = await _context.Employees.FindAsync(employeeId);
            if (emp == null) return NotFound(new { message = "Employee not found." });

            var user = await _userManager.FindByIdAsync(employeeId);
            if (user == null) return NotFound(new { message = "User identity account not found." });

            if (!await _userManager.IsInRoleAsync(user, "DepartmentHead"))
            {
                await _userManager.AddToRoleAsync(user, "DepartmentHead");
            }

            var dept = await _context.Departments.FindAsync(emp.DepartmentId);
            if (dept != null)
            {
                dept.DepartmentHeadId = emp.Id;
                dept.LastModifiedDate = DateTime.UtcNow;
                _context.Departments.Update(dept);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = $"Successfully promoted {emp.Name} to Department Head of {dept?.Name ?? "department"}." });
        }

        // Admin: Demote Department Head to Regular Employee
        [HttpPost("demote-dept-head/{employeeId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DemoteFromDeptHead(string employeeId)
        {
            var emp = await _context.Employees.FindAsync(employeeId);
            if (emp == null) return NotFound(new { message = "Employee not found." });

            var user = await _userManager.FindByIdAsync(employeeId);
            if (user != null && await _userManager.IsInRoleAsync(user, "DepartmentHead"))
            {
                await _userManager.RemoveFromRoleAsync(user, "DepartmentHead");
            }

            var depts = await _context.Departments.Where(d => d.DepartmentHeadId == employeeId).ToListAsync();
            foreach (var d in depts)
            {
                d.DepartmentHeadId = null;
                d.LastModifiedDate = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Successfully demoted {emp.Name} to regular Employee." });
        }

        // Dept Head / Admin: Add Employee to Department
        [HttpPost("dept-employee")]
        [Authorize(Roles = "Admin,SuperAdmin,DepartmentHead")]
        public async Task<IActionResult> AddDepartmentEmployee([FromBody] AddEmployeeDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Name) || dto.DepartmentId == Guid.Empty)
                return BadRequest(new { message = "Name, Email, and DepartmentId are required." });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole("DepartmentHead") && !User.IsInRole("Admin"))
            {
                var deptHeadRecord = await _context.Employees.FindAsync(currentUserId);
                if (deptHeadRecord != null && deptHeadRecord.DepartmentId != dto.DepartmentId)
                {
                    return Forbid("Department Heads can only add employees to their own department.");
                }
            }

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "A user with this email already exists." });

            var newUser = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true,
                Status = true,
                HasLoginAccess = true,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            var password = string.IsNullOrEmpty(dto.Password) ? "Employee@123" : dto.Password;
            var createResult = await _userManager.CreateAsync(newUser, password);
            if (!createResult.Succeeded)
            {
                return BadRequest(new { message = string.Join(", ", createResult.Errors.Select(e => e.Description)) });
            }

            await _userManager.AddToRoleAsync(newUser, "Employee");

            var empCount = await _context.Employees.CountAsync() + 1000;
            var empRecord = new Employee
            {
                Id = newUser.Id,
                Name = dto.Name,
                Email = dto.Email,
                EmployeeId = $"EMP-{empCount}",
                AutoGenrateId = $"EMP-{empCount}",
                DepartmentId = dto.DepartmentId,
                Designation = dto.Designation ?? "Support Specialist",
                RegisteredMobileNumber = dto.Phone ?? string.Empty,
                CreatedDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Employees.Add(empRecord);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee added successfully.", employee = empRecord });
        }

        // Dept Head / Admin: Delete Employee from Department
        [HttpDelete("dept-employee/{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,DepartmentHead")]
        public async Task<IActionResult> DeleteDepartmentEmployee(string id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound(new { message = "Employee not found." });

            emp.IsDeleted = true;
            emp.LastUpdatedDate = DateTime.UtcNow;

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Status = false;
                user.HasLoginAccess = false;
                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Employee {emp.Name} deleted successfully." });
        }
    }

    public class AddEmployeeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public Guid DepartmentId { get; set; }
        public string? Designation { get; set; }
        public string? Phone { get; set; }
    }
}
