using Dating_WebAPI.Entities;
using Dating_WebAPI.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Dating_WebAPI.Services
{
    public class TokenServices : ITokenServices
    {
        private readonly SymmetricSecurityKey _symmetricSecurityKey;
        private readonly UserManager<AppUser> userManager;

        //建構式，將Configuration注入
        public TokenServices(IConfiguration configuration, UserManager<AppUser> _userManager)
        {
            this.userManager = _userManager;
            // 記得要去appsettings.json加入"TokenKey" : "隨便什麼都好，但是要大於32個字"
            this._symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenKey"]));
        }

        //實作ITokenServices用來傳輸JWT，要去Startup註冊
        public async Task<string> CreateToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };
            var roles = await userManager.GetRolesAsync(user);
            // JwtRegisteredClaimNames 裡面並沒有Role可以用，所以使用ClaimType取代
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            var creds = new SigningCredentials(_symmetricSecurityKey, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}