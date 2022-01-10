using AutoMapper;
using Dating_WebAPI.DTOs;
using Dating_WebAPI.Entities;
using Dating_WebAPI.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.Controllers
{
    public class AccountController : BaseApIController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenServices _tokenServices;
        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenServices tokenServices, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenServices = tokenServices;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if (await UserExists(registerDTO.Email) || await UserExists(registerDTO.UserName))
            {
                // return http 400 and show message
                return BadRequest("Email or UserName is taken!");
            }
            else
            {
                var user = _mapper.Map<AppUser>(registerDTO);

                user.UserName = registerDTO.UserName.ToLower();

                var result = await _userManager.CreateAsync(user, registerDTO.Password);

                if (!result.Succeeded) return BadRequest(result.Errors);

                // add role
                var roleResult = await _userManager.AddToRoleAsync(user, "Member");

                if (!roleResult.Succeeded) BadRequest(result.Errors);

                return new UserDTO
                {
                    UserName = user.UserName,
                    Token = await _tokenServices.CreateToken(user),
                    NickName = user.NickName,
                    Gender = user.Gender
                };
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            // 如果沒有Include Photos，則在return PhotoUrl的時候會因為找不到路徑而報Null Exception。
            AppUser user = await _userManager.Users.Include(n => n.Photos).SingleOrDefaultAsync(n => n.UserName == loginDTO.UserName.ToLower());

            if (user == null) return Unauthorized("Invalid username");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);

            if (!result.Succeeded) return Unauthorized();

            return new UserDTO
            {
                UserName = user.UserName,
                Token = await _tokenServices.CreateToken(user),
                // 記得要先Include Photo不然找不到。
                PhotoUrl = user.Photos.FirstOrDefault(n => n.IsMain)?.Url,
                NickName = user.NickName,
                Gender = user.Gender
            };
        }

        private async Task<bool> UserExists(string data)
        {
            bool emailExists = await _userManager.Users.AnyAsync(n => n.Email.ToLower() == data.ToLower());
            bool userNameExists = await _userManager.Users.AnyAsync(n => n.UserName.ToLower() == data.ToLower());
            // 判斷帳號是否存在
            return emailExists || userNameExists;
        }
    }
}