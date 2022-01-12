using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dating_WebAPI.DTOs;
using Dating_WebAPI.Entities;
using Dating_WebAPI.Helpers;
using Dating_WebAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.Data
{
    // 實作共用EF方法
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public UserRepository(DataContext dataContext, IMapper mapper)
        {
            this._dataContext = dataContext;
            this._mapper = mapper;
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _dataContext.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string username)
        {
            // 取得關聯資料表的資料使用Include
            return await _dataContext.Users.Include(prop => prop.Photos).SingleOrDefaultAsync(n => n.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _dataContext.Users.Include(prop => prop.Photos).ToListAsync();
        }

        public void Update(AppUser user)
        {
            // 標記Entity已經被變更，而不是直接Update資料庫
            _dataContext.Entry(user).State = EntityState.Modified;
        }

        public async Task<MemberDTO> GetMemberAsync(string username)
        {
            return await _dataContext.Users.Where(
                n => n.UserName == username)
                .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<PageList<MemberDTO>> GetMembersAsync(UserParams userParams)
        {
            // AsNoTracking適用時機：　1.想要直接取得DB實際資料(忽略快取資料) 2.查詢結果僅供檢視(沒有異動需求)
            // 如果查詢的資料沒有異動需求，就可使用AsNoTracking方法將資料排除於追蹤清單中。
            // 另外由於查詢資料未列入追蹤，Entity Framework無須對此資料進行額外的處理，因此自然在查詢速度會有較佳的表現。

            var q = _dataContext.Users.AsQueryable();

            // 先篩選條件，到最後才project出去
            q = q.Where(n => n.UserName != userParams.CurrentUserName);
            q = q.Where(n => n.Gender == userParams.Gender);


            var minBirthday = DateTime.Today.AddYears(-userParams.MaxAge -1);
            var maxBirthday = DateTime.Today.AddYears(-userParams.MinAge);

            q = q.Where(n => n.Birthday >=minBirthday && n.Birthday <= maxBirthday);

            q = userParams.OrderBy switch
            {
                "created" => q.OrderByDescending(n => n.AccountCreateTime),
                // _ 代表Default
                _ => q.OrderByDescending(n => n.LastLoginTime)
            };

            return await PageList<MemberDTO>.CreateAsync(
                q.ProjectTo<MemberDTO>(_mapper.ConfigurationProvider).AsNoTracking(), 
                userParams.PageNumber, 
                userParams.PageSize);
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _dataContext.Users.Where(n => n.UserName == username).Select(n => n.Gender).FirstOrDefaultAsync();
        }
    }
}