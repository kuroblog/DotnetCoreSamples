using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sample1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddScoped<ITest, Test1>();
            services.AddScoped<ITest, Test2>();

            services.AddScoped<VersionFilter>();
            services.AddScoped<ServiceFilter>();
            services.AddScoped<VersionSettings>();
            services.AddScoped<ServiceSettings>();
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
                app.UseHsts();
            }

#if !DEBUG
            app.UseHttpsRedirection();
#endif
            app.UseMvc();
        }
    }

    public interface ITest
    {
        string Name { get; }

        void Print();
    }

    public class Test1 : ITest
    {
        public string Name => nameof(Test1);

        public void Print()
        {
            Trace.WriteLine(nameof(Test1));
        }
    }

    public class Test2 : ITest
    {
        public string Name => nameof(Test2);

        public void Print()
        {
            Trace.WriteLine(nameof(Test2));
        }
    }

    public class VersionSettings
    {
        public string Ver { get; set; }
    }

    public class ServiceSettings
    {
        public string Name { get; set; }
    }

    public class VersionFilter : IActionFilter
    {
        private readonly VersionSettings version;

        public VersionFilter(VersionSettings version)
        {
            this.version = version;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            object ver = null;
            var isGot = context == null ? false : context.RouteData.Values.TryGetValue("version", out ver);

            version.Ver = isGot ? ver.ToString() : "v1";

            Trace.WriteLine(new
            {
                name = nameof(VersionFilter),
                fun = MethodBase.GetCurrentMethod().Name,
                msg = $"get version is {version.Ver}."
            });
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }

    public class ServiceFilter : IActionFilter
    {
        private readonly ServiceSettings service;

        public ServiceFilter(ServiceSettings service)
        {
            this.service = service;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            object name = null;
            var isGot = context == null ? false : context.RouteData.Values.TryGetValue("service", out name);

            service.Name = isGot ? name.ToString() : "";

            Trace.WriteLine(new
            {
                name = nameof(VersionFilter),
                fun = MethodBase.GetCurrentMethod().Name,
                msg = $"get service is {service.Name}."
            });
        }

        public void OnActionExecuting(ActionExecutingContext context) { }
    }
}
