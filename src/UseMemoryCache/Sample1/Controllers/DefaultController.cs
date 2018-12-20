using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Sample1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        private readonly IMemoryCache cache;

        public DefaultController(IMemoryCache cache)
        {
            this.cache = cache;
        }

        [HttpGet, Route("test")]
        public async Task<ActionResult> Hello()
        {
            return await Task.FromResult(Ok("hello world!"));
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery]string key)
        {
            var value = string.Empty;
            var isGot = cache.TryGetValue(key, out value);
            if (isGot)
            {
                return await Task.FromResult(Ok(value));
            }

            return await Task.FromResult(NotFound());
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]CacheInfo req)
        {
            var mceo = new MemoryCacheEntryOptions();
            mceo.SetSlidingExpiration(TimeSpan.FromSeconds(10));
            //mceo.SetPriority(CacheItemPriority.NeverRemove);
            mceo.RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                cache.Set(key, $"expirated by {reason}");
            });
            cache.Set(req.key, req.json, mceo);

            return await Task.FromResult(Ok());
        }
    }

    public class CacheInfo
    {
        public string key { get; set; }

        public string json { get; set; }
    }
}
