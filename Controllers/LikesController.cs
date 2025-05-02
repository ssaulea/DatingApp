using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController(IUnitOfWork unitOfWork) : BaseApiController
    {
        [HttpPost("{targetUserId:int}")]
        public async Task<ActionResult> ToogleLike(int targetUserId)
        {
            var sourceUserId = User.GetUserId();
            
            if (sourceUserId == targetUserId) return BadRequest("You cannot like yourself");
            
            var existingLike = await unitOfWork.LikesRepository.GetUserLike(sourceUserId, targetUserId);

            if (existingLike == null)
                unitOfWork.LikesRepository.AddLike(new UserLike{ SourceUserId = sourceUserId, TargetUserId = targetUserId});
            else 
                unitOfWork.LikesRepository.DeleteLike(existingLike);

            if (!await unitOfWork.Complete()) BadRequest("Fail to update like");

            return Ok();
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
        {
            return Ok(await unitOfWork.LikesRepository.GetCurrentUserLikeIds(User.GetUserId()));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetCurrentUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await unitOfWork.LikesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users);

            return Ok(users); 
        }
    }
}
