using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helper;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(IUserRepository userRepository, IPhotoService photoService, IMapper mapper) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var users = await userRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users);

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await userRepository.GetMemberByUsernameAsync(username);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUserName());

            if (user is null)
            {
                return NotFound("User not found");
            }

            mapper.Map(memberUpdateDto, user);

            if (await userRepository.SaveAllAsync())
            {
                return NoContent();
            }

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhotoAsync(IFormFile file)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUserName());

            if (user is null)
            {
                return BadRequest("User not found");
            }

            var result = await photoService.AddPhotoAsync(file);

            if (result.Error is not null)
            {
                return BadRequest(result.Error);
            }

            var photo = new Photo
            {
                PublicId = result.PublicId,
                Url = result.SecureUrl.AbsoluteUri,
            };

            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await userRepository.SaveAllAsync())
            {
                //return mapper.Map<PhotoDto>(photo);
                return Created(photo.Url, mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Failed to add photo");
        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUserName());

            if (user is null)
            {
                return BadRequest("User not found");
            }

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo is null)
            {
                return BadRequest("Photo not found");
            }

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (currentMain is not null)
            {
                currentMain.IsMain = false;
            }

            photo.IsMain = true;

            if (await userRepository.SaveAllAsync())
            {
                return NoContent();
            }

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUserName());

            if (user is null)
            {
                return BadRequest("User not found");
            }

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo is null)
            {
                return BadRequest("Photo not found");
            }

            if (photo.PublicId is not null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);

                if (result.Error is not null)
                {
                    return BadRequest(result.Error.Message);
                }

                user.Photos.Remove(photo);

                if (await userRepository.SaveAllAsync())
                {
                    return Ok();
                }
            }

            return BadRequest("Failed to delete photo");
        }
    }
}
