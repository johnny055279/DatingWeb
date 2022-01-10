using Dating_WebAPI.Data;
using Dating_WebAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.Controllers
{
    public class BugController : BaseApIController
    {
        private readonly DataContext _dataContext;

        public BugController(DataContext dataContext)
        {
            this._dataContext = dataContext;
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }

        [HttpGet("notFound")]
        public ActionResult<AppUser> GetNotFound()
        {
            var thing = _dataContext.Users.Find(-1);

            if (thing == null)
            {
                // Return 404
                return NotFound();
            }
            else
            {
                // Return 200
                return Ok(thing);
            }
        }

        [HttpGet("serverError")]
        public ActionResult<string> GetServerError()
        {
            var thing = _dataContext.Users.Find(-1);

            // Find(-1)可以return nnull，因此當null.Tostring()時會丟出Null reference Exception
            var thingToReturn = thing.ToString();

            return thingToReturn;
        }

        [HttpGet("badRequest")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest();
        }
    }
}