using API.Dtos;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController:ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager=userManager;
        }
        
        [HttpPost]
        [Authorize(Roles = " Admin")]
        public async Task<IActionResult> CreateRole([FromForm] CreateRoleDto createRoleDto)
        {
            if(string.IsNullOrEmpty(createRoleDto.RoleName))
            {
                return BadRequest("Role name is required");
            }
            var roleExist = await _roleManager.RoleExistsAsync(createRoleDto.RoleName);
            if(roleExist)
            {
                return BadRequest("Role already exist");
            }
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(createRoleDto.RoleName));
            if(roleResult.Succeeded)
            {
                return Ok(new {message="Role Created successfully"});
            }
            return BadRequest("Role creation failed.");   
        }

        [HttpGet]
        [Authorize(Roles = " Admin")]
        public async Task<IActionResult> GetRoles()
        {
            var allRoles = await _roleManager.Roles.ToListAsync();
            var rolesWithUsersCount = new List<RoleResponseDto>();
            foreach (var role in allRoles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                var usersCount = usersInRole.Count;
                rolesWithUsersCount.Add(new RoleResponseDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    TotalUsers = usersCount
                });
            }
            return Ok(rolesWithUsersCount);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = " Admin")]
        public async Task<IActionResult> DeleteRole( string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if(role is null)
            {
                return NotFound("Role not found.");
            }
            var result = await _roleManager.DeleteAsync(role);
            if(result.Succeeded)
            {
                return Ok( new {message="Role deleted successfully."});
            }
            return BadRequest("Role deletion failed.");  
        }

        [HttpPost("assign")]
        [Authorize(Roles = " Admin")]
        public async Task<IActionResult> AssignRole([FromForm] RoleAssignDto roleAssignDto)
        {
            var user = await _userManager.FindByIdAsync(roleAssignDto.UserId);
            if(user is null)
            {
                return NotFound("User not found.");
            }
            var role =await _roleManager.FindByIdAsync(roleAssignDto.RoleId);
            if(role is null)
            {
                return NotFound("Role not found.");
            }
            var result = await _userManager.AddToRoleAsync(user,role.Name!);
            if(result.Succeeded)
            {
                return Ok(new {message="Role assigned successfully"});
            }
            var error = result.Errors.FirstOrDefault();
            return BadRequest(error!.Description);
        }
    }
}