using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController(DataContext context, ITokenService tokenService, IMapper mapper) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username))
            {
                return BadRequest("User name already exists!");
            }

            using var hmac = new HMACSHA512();

            var user = mapper.Map<AppUser>(registerDto);
            user.UserName = registerDto.Username;

            context.Users.Add(user);

            await context.SaveChangesAsync();

            return new UserDto
            {
                UserName = user.UserName,
                Token = tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender,
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await context.Users.Include(user => user.Photos).FirstOrDefaultAsync(user => user.NormalizedUserName == loginDto.UserName.ToUpper());

            if (user is null || user.UserName is null)
            {
                return Unauthorized("Invalid username");
            }

            return new UserDto
            {
                UserName = user.UserName,
                Token = tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender,
                PhotoUrl = user.Photos?.FirstOrDefault(x => x.IsMain)?.Url
            };
        }

        private async Task<bool> UserExists(string userName)
        {
            return await context.Users.AnyAsync(user => user.NormalizedUserName == userName.ToUpper());
        }
    }
}
