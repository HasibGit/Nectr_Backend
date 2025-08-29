using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController(UserManager<AppUser> userManager) : BaseApiController
    {
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await userManager.Users
                                        .OrderBy(x => x.UserName)
                                        .Select(x => new
                                        {
                                            x.Id,
                                            x.UserName,
                                            Roles = x.UserRoles.Select(r => r.Role.Name).ToList()
                                        }).ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, string roles)
        {
            if (string.IsNullOrEmpty(roles))
            {
                return BadRequest("No roles selected");
            }

            var selectedRoles = roles.Split(",").ToArray();
            var user = await userManager.FindByNameAsync(username);

            if (user is null)
            {
                return BadRequest("User not found");
            }

            var userRoles = await userManager.GetRolesAsync(user);

            var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded)
            {
                return BadRequest("Role assignment failed");
            }

            result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded)
            {
                return BadRequest("Role assignment failed");
            }

            return Ok(await userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-for-moderation")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins and moderators can see this.");
        }
    }
}

