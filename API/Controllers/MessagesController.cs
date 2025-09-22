using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helper;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController(IUnitOfWork unitOfWork, IMapper mapper) : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUserName();

            if (username == createMessageDto.RecipientUsername.ToLower())
            {
                return BadRequest("User cannot message himself");
            }

            var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null)
            {
                return BadRequest("Message sending failed");
            }

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            unitOfWork.MessageRepository.AddMessage(message);

            if (await unitOfWork.Complete())
            {
                return Ok(mapper.Map<MessageDto>(message));
            }

            return BadRequest("Failed to save message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessagesParams messagesParams)
        {
            messagesParams.Username = User.GetUserName();

            var messages = await unitOfWork.MessageRepository.GetMessagesForUser(messagesParams);

            Response.AddPaginationHeader(messages);

            return messages;
        }

        [HttpGet("thread/{recipientUserName}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string recipientUserName)
        {
            var currentUsername = User.GetUserName();

            return Ok(await unitOfWork.MessageRepository.GetMessageThread(currentUsername, recipientUserName));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();
            var message = await unitOfWork.MessageRepository.GetMessage(id);

            if (message is null)
            {
                return BadRequest("Message deletion failed");
            }

            if (message.SenderUsername != username && message.RecipientUsername != username)
            {
                return Forbid();
            }

            if (message.SenderUsername == username)
            {
                message.SenderDeleted = true;
            }

            if (message.RecipientUsername == username)
            {
                message.RecipientDeleted = true;
            }

            if (message is { SenderDeleted: true, RecipientDeleted: true })
            {
                unitOfWork.MessageRepository.DeleteMessage(message);
            }

            if (await unitOfWork.Complete())
            {
                return Ok();
            }

            return BadRequest("Message deletion failed");
        }
    }
}
