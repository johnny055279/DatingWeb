using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Dating_WebAPI.Entities
{
    // Identity預設是以字串當作PK，這裡改成int。
    public class AppUser : IdentityUser<int>
    {
        [Key]
        public string Gender { get; set; }
        public string NickName { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime AccountCreateTime { get; set; } = DateTime.Now;
        public DateTime LastLoginTime { get; set; } = DateTime.Now;
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Introduction { get; set; }
        public ICollection<Photo> Photos { get; set; }
        public ICollection<UserLike> LikeByUser { get; set; }
        public ICollection<UserLike> LikedUsers { get; set; }
        public ICollection<Message> MessagesSent { get; set; }
        public ICollection<Message> MessagesReceived { get; set; }
        public ICollection<AppUserRole> UserRoles { get; set; }
    }
}