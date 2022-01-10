using Dating_WebAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Dating_WebAPI.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApIController : ControllerBase
    {
    }
}