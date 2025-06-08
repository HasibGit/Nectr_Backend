using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(IUserRepository userRepository, IPhotoService photoService,  IMapper mapper) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await userRepository.GetMembersAsync();

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
            
            user.Photos.Add(photo);

            if (await userRepository.SaveAllAsync())
            {
                //return mapper.Map<PhotoDto>(photo);
                return Created(photo.Url, mapper.Map<PhotoDto>(photo));
            }
            
            return BadRequest("Failed to add photo");
        }
        
    }
}
