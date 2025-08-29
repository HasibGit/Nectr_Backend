using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public ActionResult GetUsersWithRoles()
        {
            return Ok("Only admins can see this.");
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-for-moderation")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins and moderators can see this.");
        }
    }
}

