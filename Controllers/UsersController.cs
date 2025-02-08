using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await userRepository.GetMembers();

            return Ok(users);
        }

        // [HttpGet("{id:int}")]
        // public async Task<ActionResult<AppUser>> GetUser(int id)
        // {
        //     var user = await userRepository.GetUserByUserIdAsync(id);

        //     if (user == null)
        //         return NotFound();

        //     return user;
        // }

        [HttpGet("{userName}")]
        public async Task<ActionResult<MemberDto>> GetUser(string userName)
        {
            var user = await userRepository.GetMember(userName);

            if (user == null)
                return NotFound();

            return user;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username == null) return BadRequest("No username found in token");

            var user = await userRepository.GetUserByUsernameAsync(username);
            if (user == null) return BadRequest("Could not fiund user");

            mapper.Map(memberUpdateDto, user);
            if (await userRepository.SaveAllAsync())
                return NoContent();

            return BadRequest("Failed to update the user");
        }
    }
}
