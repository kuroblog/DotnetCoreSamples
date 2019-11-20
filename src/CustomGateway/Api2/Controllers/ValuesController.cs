using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        
        [HttpGet]
        [Route("~/api/settings")]
        public async Task<IActionResult> GetSettings()
        {
            var printResult = new
            {
                Request.Headers
            };
            var result = StatusCode(StatusCodes.Status200OK, printResult);
            return await Task.FromResult(result);
        }

        [HttpPost]
        [Route("~/api/test")]
        public async Task<IActionResult> Test([FromBody]object body)
        {
            // var bodyString = string.Empty;
            // using (var reader = new StreamReader(Request.Body))
            // {
            //     bodyString = reader.ReadToEnd();
            // }
            var printResult = new
            {
                Request.Headers,
                Body = body
            };
            var result = StatusCode(StatusCodes.Status200OK, printResult);
            return await Task.FromResult(result);
        }
    }
}
