using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController(ILikeRepository likeRepository) : BaseApiController
    {
        [HttpPost("{targetUserId:int}")]
        public async Task<ActionResult> ToggleLike(int targetUserId)
        {
            var sourceUserId = User.GetUserId();

            if (sourceUserId == targetUserId)
            {
                return BadRequest("You cannot like yourself");
            }

            var likeStatus = await likeRepository.GetLike(sourceUserId, targetUserId);

            if (likeStatus == null)
            {
                var like = new Like
                {
                    SourceUserId = sourceUserId,
                    TargetUserId = targetUserId
                };

                likeRepository.AddLike(like);
            }
            else
            {
                likeRepository.DeleteLike(likeStatus);
            }

            if (await likeRepository.SaveChanges())
            {
                return Ok();
            }

            return BadRequest("failed to update like");
        }


        [HttpGet("ids")]
        public async Task<ActionResult<IEnumerable<int>>> GetLikeIds()
        {
            return Ok(await likeRepository.GetLikeIds(User.GetUserId()));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetLikes(string predicate)
        {
            var users = await likeRepository.GetLikes(predicate, User.GetUserId());

            return Ok(users);
        }
    }
}



