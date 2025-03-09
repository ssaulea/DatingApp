using System.ComponentModel.DataAnnotations;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    public class MessagesController(IMessagesRepository messagesRepository, IUserRepository userRepository, IMapper mapper) : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUserName();
            if (createMessageDto.RecipientUsername == username) return BadRequest("You cannot  message yourself");

            var sender = await userRepository.GetUserByUsernameAsync(username);
            var recipient = await userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (sender is null || recipient is null) return BadRequest("Cannot send message at this time");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = username,
                RecipientUsername = createMessageDto.RecipientUsername,
                Content = createMessageDto.Content
            };

            await messagesRepository.AddMessage(message);

            if (!await messagesRepository.SaveAllAsync()) return BadRequest("Fail to save message");

            return Ok(mapper.Map<MessageDto>(message));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUserName();

            var messages = await messagesRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages);

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread([Required] string username)
        {
            var currentUsername = User.GetUserName();

            return Ok(await messagesRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var username = User.GetUserName();
            var message = await messagesRepository.GetMessage(id);
            if (message == null) return BadRequest("Cannot delete this message");

            if (message.SenderUsername != username && message.RecipientUsername != username)
                return Forbid();

            if (message.SenderUsername == username) message.SenderDeleted = true;
            if (message.RecipientUsername == username) message.RecipientDeleted = true;

            if (message is {SenderDeleted: true, RecipientDeleted: true})
            {
                messagesRepository.DeleteMessage(message);
            }

            if (await messagesRepository.SaveAllAsync()) return Ok();
            else return BadRequest("Problem deleting the message");
        } 
    }
}
