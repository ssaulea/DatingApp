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
    public class UsersController(IUserRepository userRepository) : BaseApiController
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
    }
}
