using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dating_WebAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Dating_WebAPI.Controllers
{
    public class AdminController : BaseApIController
    {
        public readonly UserManager<AppUser> userManager;

        public AdminController(UserManager<AppUser> _userManager)
        {
            this.userManager = _userManager;
        }

        // 設定權限
       [Authorize(Policy = "RequireAdminRole")]
       [HttpGet("user-with-roles")]
       public async Task<ActionResult> GetUsersWithRole()
        {
            // let admin can edit user's role
            var users = await userManager.Users
                .Include(n => n.UserRoles)
                .ThenInclude(n => n.Role)
                .OrderBy(n => n.UserName)
                .Select(n => new {
                n.Id,
                UserName = n.UserName,
                // AppRole繼承了IdentityRole<int>, 所以可以找到Name
                Roles = n.UserRoles.Select(n => n.Role.Name).ToList()
                }).ToListAsync();
            return Ok(users);
        }

        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            // 權限可能有多個，使其轉為陣列
            var selectRoles = roles.Split(",").ToArray();
            // 找到符合的使用者
            var user = await userManager.FindByNameAsync(username);

            if (user == null) return NotFound("Could not find user");

            // 取得目前權限有哪些
            var userRoles = await userManager.GetRolesAsync(user);
            // 利用差集將selectRoles未包含在其中的加入
            var result = await userManager.AddToRolesAsync(user, selectRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Failed to add roles");

            // 再一次利用差集將selectRoles以外的權限刪除
            result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectRoles));

            if (!result.Succeeded) return BadRequest("Failed to remove roles");

            return Ok(await userManager.GetRolesAsync(user));

        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photo-to-moderate")]
        public ActionResult GetModeratePhoto()
        {
            return Ok("Admins or Moderators can see this.");
        }
    }
}

