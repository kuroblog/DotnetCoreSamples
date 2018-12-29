using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            services.AddMvc(cfg =>
            {
                //cfg.Filters.Add(RouteNameFilter);
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddScoped<ITest, Test1>();
            services.AddScoped<ITest, Test2>();

            services.AddScoped<RouteNameSettings>();
            services.AddScoped<RouteNameFilter>();
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

            app.UseHttpsRedirection();
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

    public static class ITestExtensions
    {
        public static ITest Get(this IEnumerable<ITest> tests, string name)
        {
            return tests?.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class RouteNameSettings
    {
        public string Name { get; set; }
    }

    public class RouteNameFilter : IActionFilter
    {
        private readonly RouteNameSettings rms;

        public RouteNameFilter(RouteNameSettings rms)
        {
            this.rms = rms;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            object name = null;
            var isGot = context == null ? false : context.RouteData.Values.TryGetValue("name", out name);

            rms.Name = isGot ? name.ToString() : string.Empty;
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
