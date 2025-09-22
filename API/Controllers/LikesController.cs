using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController(IUnitOfWork unitOfWork) : BaseApiController
    {
        [HttpPost("{targetUserId:int}")]
        public async Task<ActionResult> ToggleLike(int targetUserId)
        {
            var sourceUserId = User.GetUserId();

            if (sourceUserId == targetUserId)
            {
                return BadRequest("You cannot like yourself");
            }

            var likeStatus = await unitOfWork.LikeRepository.GetLike(sourceUserId, targetUserId);

            if (likeStatus == null)
            {
                var like = new Like
                {
                    SourceUserId = sourceUserId,
                    TargetUserId = targetUserId
                };

                unitOfWork.LikeRepository.AddLike(like);
            }
            else
            {
                unitOfWork.LikeRepository.DeleteLike(likeStatus);
            }

            if (await unitOfWork.Complete())
            {
                return Ok();
            }

            return BadRequest("failed to update like");
        }


        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<int>>> GetLikeIds()
        {
            return Ok(await unitOfWork.LikeRepository.GetLikeIds(User.GetUserId()));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await unitOfWork.LikeRepository.GetLikes(likesParams);

            Response.AddPaginationHeader(users);

            return Ok(users);
        }
    }
}



