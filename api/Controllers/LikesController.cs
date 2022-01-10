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
    public class LikesController : BaseApIController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;

        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            this._userRepository = userRepository;
            this._likesRepository = likesRepository;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string userName)
        {
            var sourceUserId = User.GetUserId();
            var likeUser = await _userRepository.GetUserByUserNameAsync(userName);
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

            if (likeUser == null) return NotFound();

            if (sourceUser.UserName == userName) return BadRequest("不要對自己點讚!!");

            var userLike = await _likesRepository.GetUserLike(sourceUserId, likeUser.Id);

            if (userLike != null) return BadRequest("你已經按過讚惹!");

            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikeUserId = likeUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("點讚失敗!");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDTO>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();

            var users = await _likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }
    }
}