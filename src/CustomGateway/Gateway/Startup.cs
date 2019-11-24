using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gateway.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) =>
            services
                .AddSingleton(Configuration)
                .AddHttpClient()
                .AddCustom()
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

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
