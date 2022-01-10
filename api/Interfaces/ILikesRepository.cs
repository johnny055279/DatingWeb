using Dating_WebAPI.DTOs;
using Dating_WebAPI.Entities;
using Dating_WebAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int likeUserId);

        Task<AppUser> GetUserWithLikes(int userId);

        Task<PageList<LikeDTO>> GetUserLikes(LikesParams likesParams);
    }
}