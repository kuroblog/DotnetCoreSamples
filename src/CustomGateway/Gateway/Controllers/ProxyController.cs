using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Gateway.Controllers
{
    [Route("api/gateway/{service}/{*path}")]
    [ApiController]
    public class ProxyController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly ServiceSetting[] serviceSettings;
        private readonly IHttpClientFactory httpClientFactory;

        public ProxyController(
            IConfiguration config,
            IHttpClientFactory httpClientFactory)
        {
            this.config = config;
            serviceSettings = this.config?.GetServiceSettings();
            
            this.httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        [Route("~/api/settings")]
        public async Task<IActionResult> GetSettings()
        {
            var printResult = new
            {
                serviceSettings
            };

            var result = StatusCode(StatusCodes.Status200OK, printResult);
            
            return await Task.FromResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> Get(string service, string path) => await doProxyRequest(service, path, HttpMethod.Get);

        [HttpPost]
        public async Task<IActionResult> Post(string service, string path) => await doProxyRequest(service, path, HttpMethod.Post);

        [HttpPut]
        public async Task<IActionResult> Put(string service, string path) => await doProxyRequest(service, path, HttpMethod.Put);

        [HttpDelete]
        public async Task<IActionResult> Delete(string service, string path) => await doProxyRequest(service, path, HttpMethod.Delete);

        private async Task<IActionResult> doProxyRequest(string serviceName, string servicePath, HttpMethod method)
        {
            try
            {
                var proxySettings = serviceSettings.GetServiceSetting(serviceName);

                var proxyRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri(Path.Combine(proxySettings.Host, servicePath, Request.QueryString.Value)),
                    Method = method
                };

                proxySettings.HeadFilters?.ToList().ForEach(setting =>
                {
                    if (Request.Headers.TryGetValue(setting.Key, out StringValues originalHeadValues))
                    {
                        proxyRequest.Headers.TryAddWithoutValidation(setting.Key, originalHeadValues.ToArray());
                    }
                    else
                    {
                        proxyRequest.Headers.TryAddWithoutValidation(setting.Key, setting.Value);
                    }
                });

                if (Request.Body != null && Request.Body.CanRead)
                {
                    var bodyString = await Request.GetRawBodyStringAsync();
                    proxyRequest.Content = new StringContent(bodyString, Encoding.UTF8, Request.ContentType);
                }

                var client = httpClientFactory.CreateClient();

                var response = await client.SendAsync(proxyRequest);

                return new ProxyResult(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.GetErrorMessages());
            }
        }
    }
}

namespace Gateway
{
    public class ServiceSetting
    {
        public string Name { get; set; }

        public string Host { get; set; }

        public Dictionary<string, string[]> HeadFilters { get; set; }
    }

    public class ProxyResult : IActionResult
    {
        private readonly HttpResponseMessage response;

        public ProxyResult(HttpResponseMessage response) => this.response = response;

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)response.StatusCode;

            if (response.Content.Headers.ContentType != null)
            {
                context.HttpContext.Response.ContentType = response.Content.Headers.ContentType.MediaType;
            }

            //response.Headers?.ToList().ForEach(p => context.HttpContext.Response.Headers.TryAdd(p.Key, new Microsoft.Extensions.Primitives.StringValues(p.Value.ToArray())));

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                await stream.CopyToAsync(context.HttpContext.Response.Body);
                await context.HttpContext.Response.Body.FlushAsync();
            }
        }
    }
}