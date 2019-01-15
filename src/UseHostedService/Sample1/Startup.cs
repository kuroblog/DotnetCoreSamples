using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
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

            services.AddHostedService<TimedHostedService>();
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

    public class TimedHostedService : Microsoft.Extensions.Hosting.IHostedService, IDisposable
    {
        private ILogger logger;
        private Timer timer;

        public void Dispose()
        {
            timer?.Dispose();
        }

        public TimedHostedService(ILogger<TimedHostedService> logger)
        {
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("{0} timed background service is starting.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fffffff"));

            timer = new Timer(p =>
            {
                logger.LogInformation("{0} timed background service is working.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fffffff"));

                //Task.Delay(TimeSpan.FromSeconds(6)).Wait();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("{0} timed background service is stopping.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fffffff"));

            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
