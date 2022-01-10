using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.DTOs
{
    // DTO指資料傳遞的物件，只定義，並不具備操作行為，用於傳遞訊息給client。
    public class RegisterDTO
    {
        [Required]
        [StringLength(20)]
        public string UserName { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4)]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string NickName { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        [StringLength(100)]
        [Required]
        public string LookingFor { get; set; }

        [StringLength(100)]
        [Required]
        public string Interests { get; set; }

        [StringLength(200)]
        [Required]
        public string Introduction { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }
    }
}