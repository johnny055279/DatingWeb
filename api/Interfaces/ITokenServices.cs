using Dating_WebAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.Interfaces
{
    // 建立Interface用來進行測試較佳
    public interface ITokenServices
    {
        Task<string> CreateToken(AppUser user);
    }
}