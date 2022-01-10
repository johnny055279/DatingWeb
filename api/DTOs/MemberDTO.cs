using Dating_WebAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.DTOs
{
    public class MemberDTO
    {
        public int UserId { get; set; }
        public string Gender { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string NickName { get; set; }
        public int Age { get; set; }
        public DateTime AccountCreateTime { get; set; }
        public DateTime LastLoginTime { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Introduction { get; set; }

        // 為了防止EF之間發生死循環(Object cycle)，使用DTO在Return之前先Shape我們的資料
        public ICollection<PhotoDTO> Photos { get; set; }

        //用來接Photo EF的欄位。需要另外用AppMapper設定(Profile裡)
        public string PhotoUrl { get; set; }
    }
}