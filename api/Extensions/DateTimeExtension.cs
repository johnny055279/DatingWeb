using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.Extensions
{
    public static class DateTimeExtension
    {
        public static int CalculateAge(this DateTime birthday)
        {
            var today = DateTime.Today;
            var age = today.Year - birthday.Year;
            // 判斷生日大於當日，則還沒有出生。
            if (birthday.Date > today.AddYears(-age))
            {
                age--;
            }
            return age;
        }
    }
}