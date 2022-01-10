using Dating_WebAPI.DTOs;
using Dating_WebAPI.Entities;
using Dating_WebAPI.Extensions;
using Dating_WebAPI.Helpers;
using Dating_WebAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _dataContext;

        public LikesRepository(DataContext dataContext)
        {
            this._dataContext = dataContext;
        }

        async Task<UserLike> ILikesRepository.GetUserLike(int sourceUserId, int likeUserId)
        {
            // 因為Likes資料表是複合式主鍵，所以要把兩個都加進去。
            return await _dataContext.Likes.FindAsync(sourceUserId, likeUserId);
        }

        async Task<PageList<LikeDTO>> ILikesRepository.GetUserLikes(LikesParams likesParams)
        {
            var users = _dataContext.Users.OrderBy(n => n.UserName).AsQueryable();
            var likes = _dataContext.Likes.AsQueryable();

            if (likesParams.Predicate == "liked")
            {
                likes = likes.Where(n => n.SourceUserId == likesParams.UserId);
                users = likes.Select(n => n.LikeUser);
            }

            if (likesParams.Predicate == "likeBy")
            {
                likes = likes.Where(n => n.LikeUserId == likesParams.UserId);
                users = likes.Select(n => n.SourceUser);
            }

            var likeUsers = users.Select(n => new LikeDTO
            {
                UserName = n.UserName,
                NickName = n.NickName,
                Age = n.Birthday.CalculateAge(),
                PhotoUrl = n.Photos.FirstOrDefault(n => n.IsMain == true).Url,
                City = n.City,
                Id = n.Id
            });

            return await PageList<LikeDTO>.CreateAsync(likeUsers, likesParams.PageNumber, likesParams.PageSize);
        }

        async Task<AppUser> ILikesRepository.GetUserWithLikes(int userId)
        {
            return await _dataContext.Users.Include(n => n.LikedUsers).FirstOrDefaultAsync(n => n.Id == userId);
        }
    }
}