using Dating_WebAPI.DTOs;
using Dating_WebAPI.Entities;
using Dating_WebAPI.Extensions;
using Dating_WebAPI.Helpers;
using Dating_WebAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Dating_WebAPI.Controllers
{
    [Authorize]
    public class UsersController : BaseApIController
    {
        private IUnitOfWork unitOfWork;
        private readonly AutoMapper.IMapper _mapper;
        private readonly IPhotosServices _photosServices;

        public UsersController(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper, IPhotosServices photosServices)
        {
            this.unitOfWork = unitOfWork;
            this._mapper = mapper;
            this._photosServices = photosServices;
        }


        //如果是從QueryString來的就要加上[FromQuery]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery]UserParams userParams)
        {

            // 設定初始值
            var gender = await unitOfWork.userRepository.GetUserGender(User.GetUserName());
            
            userParams.CurrentUserName = User.GetUserName();

            if(string.IsNullOrEmpty(userParams.Gender)){
                // 如果登入是女性，就顯示男性，反之亦然。
                userParams.Gender = gender == "male" ? "female" : "male";
            }

            var users = await unitOfWork.userRepository.GetMembersAsync(userParams);
            
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);



            // 不加Ok會有形別上的錯誤。
            return Ok(users);
        }

        //api/users/username 並指定這個Rout的名字
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {
            return await unitOfWork.userRepository.GetMemberAsync(username);
        }

        // Updating並不需要從Client傳送Object過來，因為Client已經有所有的Data聯繫Entity
        // 這裡是告訴API什麼東西被Update了
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            // 抓Token的資料作比對。
            var user = await unitOfWork.userRepository.GetUserByUserNameAsync(User.GetUserName());

            // Map可以直接幫助我們讓DTO與Entity做對應，不然就要寫user.City = memberUpdateDTO.City....等。
            _mapper.Map(memberUpdateDTO, user);

            unitOfWork.userRepository.Update(user);

            if (await unitOfWork.Complete()) return NoContent();

            return BadRequest("資料更新錯誤");
        }

        [HttpPost("addPhoto")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            // 取得使用者
            AppUser user = await unitOfWork.userRepository.GetUserByUserNameAsync(User.GetUserName());

            // 上傳照片
            var result = await _photosServices.AddPhotoAsync(file);

            // 檢查是否有錯誤
            if (result.Error != null) return BadRequest(result.Error.Message);

            // 設定資料庫模型
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            // 因為AppUser裡面的Photos是ICollection<>，所以可以使用Count做計算。
            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await unitOfWork.Complete())
            {
                // 回傳200固然是好，但是不必每次要抓照片都要從getuser裡面去把照片抓出來，正確的add東西回傳應該是201。
                // 所以要使Header加上Location去識別這次上傳的是什麼。
                // return _mapper.Map<PhotoDTO>(photo);

                // Create回傳201，第一個是Routing的名字([HttpGet]中可以指定名字)，第二個放routing要傳的參數，第三個放return回來的東西
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDTO>(photo));
            }

            return BadRequest("新增照片時發生錯誤!");
        }

        [HttpPut("setMainPhoto/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            AppUser user = await unitOfWork.userRepository.GetUserByUserNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(n => n.Id == photoId);

            if (photo.IsMain) return BadRequest("照片已設為主要。");

            var currentMain = user.Photos.FirstOrDefault(n => n.IsMain);

            if (currentMain != null)
            {
                currentMain.IsMain = false;
            }

            photo.IsMain = true;

            if (await unitOfWork.Complete()) return NoContent();

            return BadRequest("照片設定失敗!");
        }

        [HttpDelete("deletePhoto/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            AppUser user = await unitOfWork.userRepository.GetUserByUserNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(n => n.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("你不能刪除主要的照片");

            // 有些照片沒有PublicId
            if (photo.PublicId != null)
            {
                var result = await _photosServices.DeletePhotoAsync(photo.PublicId);

                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await unitOfWork.Complete()) return Ok();

            return BadRequest("刪除照片失敗!");
        }
    }
}