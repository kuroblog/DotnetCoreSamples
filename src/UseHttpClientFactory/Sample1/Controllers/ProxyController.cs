using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Sample1.Controllers
{
    // default
    [Route("api/{version}/{service}/[controller]")]
    [ApiController, ServiceFilter(typeof(VersionFilter)), ServiceFilter(typeof(ServiceFilter))]
    public class ProxyController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Get([FromQuery]string version, [FromQuery]string service)
        {
            return await Task.FromResult(Ok(new
            {
                fun = MethodBase.GetCurrentMethod().Name,
                ver = version,
                svc = service
            }));
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromQuery]string version, [FromQuery]string service)
        {
            return await Task.FromResult(Ok(new
            {
                fun = MethodBase.GetCurrentMethod().Name,
                ver = version,
                svc = service
            }));
        }

        [HttpPut]
        public async Task<ActionResult> Put([FromQuery]string version, [FromQuery]string service)
        {
            return await Task.FromResult(Ok(new
            {
                fun = MethodBase.GetCurrentMethod().Name,
                ver = version,
                svc = service
            }));
        }

        [HttpDelete]
        public async Task<ActionResult> Delete([FromQuery]string version, [FromQuery]string service)
        {
            return await Task.FromResult(Ok(new
            {
                fun = MethodBase.GetCurrentMethod().Name,
                ver = version,
                svc = service
            }));
        }
    }
}
