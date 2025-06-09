using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            userParams.CurrentUserName = User.GetUsername();
            var users = await unitOfWork.UserRepository.GetMembers(userParams);
            Response.AddPaginationHeader(users);
            return Ok(users);
        }

        [HttpGet("{userName}")]
        public async Task<ActionResult<MemberDto>> GetUser(string userName)
        {
            var currentUserName = User.GetUsername();

            var user = await unitOfWork.UserRepository.GetMemberAsync(userName, isCurrentUser: userName == currentUserName);

            if (user == null)
                return NotFound();

            return user;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return BadRequest("Cannot find user");

            mapper.Map(memberUpdateDto, user);
            if (await unitOfWork.Complete())
                return NoContent();

            return BadRequest("Failed to update the user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return BadRequest("Cannot not find user");
            var result = await photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error);
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            user.Photos.Add(photo);
            if (await unitOfWork.Complete())
                return CreatedAtAction(nameof(GetUser), new { username = user.UserName}, mapper.Map<PhotoDto>(photo));
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return BadRequest("Cannot not find user");
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null || photo.IsMain || !photo.IsApproved) return BadRequest("Cannot set this photo to be main");
            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;
            if (await unitOfWork.Complete()) return NoContent();
            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await unitOfWork.UserRepository.GetUserByPhotoId(photoId);
            if (user == null) return BadRequest("User not found for this photo");
            if (user.UserName != User.GetUsername()) return Unauthorized("You cannot delete this photo");
            var photo = user.Photos.First(x => x.Id == photoId);
            if (photo.IsMain) return BadRequest("Cannot delete this photo");
            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error);
            }
            user.Photos.Remove(photo);
            if (await unitOfWork.Complete()) return Ok();
            return BadRequest("Problem deleting photo");
        }
    }
}
