using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Gateway.Services;
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
        public async Task<IActionResult> Get(string service, string path) => await DoService(service, path, HttpMethod.Get);

        [HttpPost]
        public async Task<IActionResult> Post(string service, string path) => await DoService(service, path, HttpMethod.Post);

        [HttpPut]
        public async Task<IActionResult> Put(string service, string path) => await DoService(service, path, HttpMethod.Put);

        [HttpDelete]
        public async Task<IActionResult> Delete(string service, string path) => await DoService(service, path, HttpMethod.Delete);

        private async Task<IActionResult> DoService(string serviceName, string servicePath, HttpMethod method)
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

                var client = httpClientFactory.CreateClient("mock");

                var response = await client.SendAsync(proxyRequest);

                return new ProxyResult(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.GetErrorMessages());
            }
        }
    }

    [Route("api/v2/gateway/{service}/{*path}")]
    [ApiController]
    public class ProxyV2Controller : ControllerBase
    {
        private readonly IProxyService proxyService;

        public ProxyV2Controller(IProxyService proxyService)
        {
            this.proxyService = proxyService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string service, string path) => await DoService(service, path, HttpMethod.Get);

        [HttpPost]
        public async Task<IActionResult> Post(string service, string path) => await DoService(service, path, HttpMethod.Post);

        [HttpPut]
        public async Task<IActionResult> Put(string service, string path) => await DoService(service, path, HttpMethod.Put);

        [HttpDelete]
        public async Task<IActionResult> Delete(string service, string path) => await DoService(service, path, HttpMethod.Delete);

        private async Task<IActionResult> DoService(string serviceName, string servicePath, HttpMethod method)
        {
            try
            {
                var response = await proxyService.CallServiceAsync((Request, serviceName, servicePath));

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

// TODO: 待实现:将 ProxyController 部分的 doProxyRequest 函数提取到这里以单个接口和单个实现类的方式来实现
namespace Gateway.Services
{
    public interface IProxyService
    {
        Task<HttpResponseMessage> CallServiceAsync((HttpRequest gatewayRequest, string name, string path) serviceInfo);
    }

    public class ProxyService : IProxyService
    {
        private readonly IConfiguration config;
        private readonly ServiceSetting[] serviceSettings;
        private readonly IHttpClientFactory httpClientFactory;

        public ProxyService(
            IConfiguration config,
            IHttpClientFactory httpClientFactory)
        {
            this.config = config;
            serviceSettings = this.config?.GetServiceSettings();

            this.httpClientFactory = httpClientFactory;
        }

        protected virtual async Task<HttpRequestMessage> GetServiceRequestAsync((HttpRequest gatewayRequest, string name, string path) serviceInfo)
        {
            var originalRequest = serviceInfo.gatewayRequest;
            var serviceName = serviceInfo.name;
            var servicePath = serviceInfo.path;

            var proxySettings = serviceSettings.GetServiceSetting(serviceName);

            var serviceRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(Path.Combine(proxySettings.Host, servicePath, originalRequest.QueryString.Value)),
                Method = new HttpMethod(originalRequest.Method)
            };

            proxySettings.HeadFilters?.ToList().ForEach(setting =>
            {
                if (originalRequest.Headers.TryGetValue(setting.Key, out StringValues originalHeadValues))
                {
                    serviceRequest.Headers.TryAddWithoutValidation(setting.Key, originalHeadValues.ToArray());
                }
                else
                {
                    serviceRequest.Headers.TryAddWithoutValidation(setting.Key, setting.Value);
                }
            });

            if (originalRequest.Body != null && originalRequest.Body.CanRead)
            {
                var bodyString = await originalRequest.GetRawBodyStringAsync();
                serviceRequest.Content = new StringContent(bodyString, Encoding.UTF8, originalRequest.ContentType);
            }

            return await Task.FromResult(serviceRequest);
        }

        public virtual async Task<HttpResponseMessage> CallServiceAsync((HttpRequest gatewayRequest, string name, string path) serviceInfo)
        {
            var newRequest = await GetServiceRequestAsync(serviceInfo);

            var client = httpClientFactory.CreateClient("mock");

            var response = await client.SendAsync(newRequest);

            return await Task.FromResult(response);
        }
    }
}

// TODO: 基于接口的多个实现(不适用,API 上线时需要更新网关的代理服务类)
namespace Gateway.Services.Tests
{
    public class Api1ProxyService : ProxyService, IProxyService
    {
        public Api1ProxyService(
            IConfiguration config,
            IHttpClientFactory httpClientFactory) : base(config, httpClientFactory) { }
    }

    public class Api2ProxyService : ProxyService, IProxyService
    {
        public Api2ProxyService(
            IConfiguration config,
            IHttpClientFactory httpClientFactory) : base(config, httpClientFactory) { }
    }
}

// TODO: 基于 CommandHandler 来设计(待完善)
namespace Gateway.Proxy.Tests
{
    public class ProxyService
    {
        public virtual HttpResponseMessage CallService() => null;
    }

    public interface IProxyServiceHandler<T> where T : ProxyService
    {
        void Execute(T ProxyService);
    }

    public class Api1ProxyService : ProxyService { }

    public class Api1ProxyServiceHandler : IProxyServiceHandler<Api1ProxyService>
    {
        public void Execute(Api1ProxyService ProxyService) => ProxyService.CallService();
    }

    public class Api2ProxyService : ProxyService { }

    public class Api2ProxyServiceHandler : IProxyServiceHandler<Api2ProxyService>
    {
        public void Execute(Api2ProxyService ProxyService) => ProxyService.CallService();
    }

    public interface IProxyServiceFactory
    {
        IProxyServiceHandler<T> GetProxyServiceHandlerFrom<T>() where T : ProxyService;
    }

    public class ProxyServiceFactory : IProxyServiceFactory
    {
        public IProxyServiceHandler<T> GetProxyServiceHandlerFrom<T>() where T : ProxyService
        {
            var handlers = GetHandlerTypes<T>().ToList();

            //var cmdHandler = handlers.Select(handler => (ICommandHandler<T>)ObjectFactory.GetInstance(handler)).FirstOrDefault();

            var cmdHandler = Activator.CreateInstance(handlers.FirstOrDefault()) as IProxyServiceHandler<T>;

            return cmdHandler;
        }

        private IEnumerable<Type> GetHandlerTypes<T>() where T : ProxyService
        {
            var handlers =
                typeof(IProxyServiceHandler<>).Assembly.GetExportedTypes()
                    .Where(x => x.GetInterfaces().Any(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(IProxyServiceHandler<>)))
                   .Where(h => h.GetInterfaces().Any(ii => ii.GetGenericArguments().Any(aa => aa == typeof(T)))).ToList();
            return handlers;
        }
    }
}