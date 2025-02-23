using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController(ILikesRepository likesRepository) : BaseApiController
    {
        [HttpPost("{targetUserId:int}")]
        public async Task<ActionResult> ToogleLike(int targetUserId)
        {
            var sourceUserId = User.GetUserId();
            
            if (sourceUserId == targetUserId) return BadRequest("You cannot like yourself");
            
            var existingLike = await likesRepository.GetUserLike(sourceUserId, targetUserId);

            if (existingLike == null)
                likesRepository.AddLike(new UserLike{ SourceUserId = sourceUserId, TargetUserId = targetUserId});
            else 
                likesRepository.DeleteLike(existingLike);

            if (!await likesRepository.SaveChanges()) BadRequest("Fail to update like");

            return Ok();
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
        {
            return Ok(await likesRepository.GetCurrentUserLikeIds(User.GetUserId()));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetCurrentUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users);

            return Ok(users); 
        }
    }
}
