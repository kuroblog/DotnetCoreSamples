using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        [Route("hello")]
        public async Task<IActionResult> Hello()
        {
            return await Task.FromResult(Ok($"hello world."));
        }

        [HttpGet]
        [Route("test1")]
        public async Task<IActionResult> Test1(string p1)
        {
            var result = Ok($"{Guid.NewGuid().ToString("N")}: p1={p1}");
            return await Task.FromResult(result);
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
        public async Task<IActionResult> Test()
        {
            var bodyString = string.Empty;
            using (var reader = new StreamReader(Request.Body))
            {
                bodyString = reader.ReadToEnd();
            }
            var printResult = new
            {
                Request.Headers,
                Body = bodyString
            };
            var result = StatusCode(StatusCodes.Status200OK, printResult);
            return await Task.FromResult(result);
        }
    }
}
