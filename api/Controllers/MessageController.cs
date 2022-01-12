using AutoMapper;
using Dating_WebAPI.DTOs;
using Dating_WebAPI.Entities;
using Dating_WebAPI.Extensions;
using Dating_WebAPI.Helpers;
using Dating_WebAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.Controllers
{
    [Authorize]
    public class MessageController : BaseApIController
    {
        private readonly IMapper _mapper;

        private readonly IUnitOfWork unitOfWork;

        public MessageController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUserName();

            if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("你不能發送訊息給自己!");

            var sender = await unitOfWork.userRepository.GetUserByUserNameAsync(username);
            var recipient = await unitOfWork.userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            unitOfWork.messageRepository.AddMessage(message);

            if (await unitOfWork.Complete()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("傳送訊息失敗!");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUserName();

            var message = await unitOfWork.messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(message.CurrentPage, message.PageSize, message.TotalCount, message.TotalPages);

            return message;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();

            var message = await unitOfWork.messageRepository.GetMessage(id);

            if(message.Sender.UserName != username && message.Recipient.UserName != username) return Unauthorized();

            if(message.Sender.UserName == username) message.SenderDeleted = true;

            if(message.Recipient.UserName == username) message.RecipientDeleted = true;

            // 如果雙方都有刪除記號，此message正式刪除。
            if(message.SenderDeleted && message.RecipientDeleted) unitOfWork.messageRepository.DeleteMessage(message);

            if(await unitOfWork.Complete()) return Ok();

            return BadRequest("刪除訊息時發生錯誤!");

        }
    }
}