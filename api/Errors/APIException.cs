using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.Errors
{
    public class APIException
    {
        public APIException(int statusCode, string message = null, string detail = null)
        {
            StatusCode = statusCode;
            Message = message;
            Detail = detail;
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Detail { get; set; }
    }
}