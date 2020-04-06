using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Gateway.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace Gateway
{
    public class Startup
    {
        // ILoggerFactory in dotent core 2.x.x is Successful
        // in 3.x.x is error
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;

            // dotnet core 2.x.x
            logger = loggerFactory.CreateLogger<Startup>();
        }

        private ILogger<Startup> logger;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // this is ILoggerFactory on dotnet core 3.x.x
            // var logger = LoggerFactory.Create(o => o.AddDebug()).CreateLogger<Startup>();

            var fallbackResponse = new HttpResponseMessage
            {
                Content = new StringContent("fallback"),
                StatusCode = HttpStatusCode.TooManyRequests
            };

            services.AddHttpClient("mock", o =>
            {
                o.BaseAddress = new Uri("http://localhost:5009");
            })
            .AddPolicyHandler(
                Policy<HttpResponseMessage>
                    .Handle<Exception>()
                    .FallbackAsync(new Func<HttpResponseMessage>(() =>
                    {
                        // the example can be return cu
                        return fallbackResponse;
                    }).Invoke(), async o =>
                     {
                         logger.LogWarning($"fallback here {o.Exception.Message}");

                         await Task.CompletedTask;
                     }))
            .AddPolicyHandler(
                Policy<HttpResponseMessage>
                    .Handle<Exception>()
                    .CircuitBreakerAsync(
                        2,
                        TimeSpan.FromSeconds(4),
                        async (rs, ts) =>
                        {
                            logger.LogWarning($"break here {ts.TotalMilliseconds}");

                            await Task.CompletedTask;
                        },
                        async () =>
                        {
                            logger.LogWarning($"reset here");

                            await Task.CompletedTask;
                        }))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));

            services
                .AddSingleton(Configuration)
                .AddHttpClient()
                .AddCustom()
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }

    public static class CustomStartupExtension
    {
        public static ServiceSetting[] GetServiceSettings(this IConfiguration config, string name = "ServiceSettings") =>
            config.GetSection(name)?.Get<ServiceSetting[]>();

        public static ServiceSetting GetServiceSetting(this ServiceSetting[] settings, string name) =>
            settings?.FirstOrDefault(setting => setting.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public static async Task<string> GetRawBodyStringAsync(this HttpRequest request, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            request.Body.Flush();
            using (var reader = new StreamReader(request.Body, encoding))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static string[] GetErrorMessages(this Exception error, List<string> errorMessages = null)
        {
            if (errorMessages == null)
            {
                errorMessages = new List<string>();
            }

            errorMessages.Add(error.Message);
            if (error.InnerException != null)
            {
                GetErrorMessages(error.InnerException, errorMessages);
            }

            return errorMessages.ToArray();
        }

        public static string GetFullErrorMessage(this Exception error, string separator) => string.Join(separator, error.GetErrorMessages());

        public static IServiceCollection AddCustom(this IServiceCollection services) =>
            services.AddScoped<IProxyService, ProxyService>();
    }
}
