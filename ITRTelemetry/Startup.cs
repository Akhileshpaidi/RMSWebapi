using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySQLProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;

using System.IO;
using Microsoft.AspNetCore.Http;
using ITR_TelementaryAPI.Hubs;
using ITR_TelementaryAPI.Models.SignalR;
using ITR_TelementaryAPI.Models.SignalRTickData;
using MySql.Data.MySqlClient;
using ITRTelemetry.HubConfig;
using Microsoft.AspNetCore.HttpOverrides;
using ITRTelemetry.Hubs;
using ITRTelemetry.Models.SignalRData;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ITRTelemetry.Controllers;

using System.Text;

namespace ITRTelemetry
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
            services.AddCors(options => options.AddPolicy("CorsPolicy",
              builder =>
              {
                  builder
                  .WithOrigins("http://192.168.1.119")
.WithOrigins("http://localhost:4200")
 //.WithOrigins("https://grcgov.pfs-ess.com")
                       // .WithOrigins("http://grma.neemus.com")
                       //  .WithOrigins("https://grcgov.pfs-ess.com")
                         //.WithOrigins("http://localhost:4200")
                         .AllowAnyMethod()
                         .AllowAnyHeader()
                         .AllowCredentials();
              }));
            services.AddScoped<BatchComplianceGeneration>();  // Register controller as a service
            //services.AddDbContext<MySqlDBContext>();         // Register DB Context
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Register HttpContextAccessor
            services.AddHostedService<DailyTaskService>();

            services.AddDbContext<MySqlDBContext>(options =>
            options.UseMySQL(Configuration.GetConnectionString("myDb1"))); // Ensure you have the correct MySQL provider


            services.AddLogging();
            services.AddHttpContextAccessor();
            services.AddSignalR()
                 .AddNewtonsoftJsonProtocol(options => { options.PayloadSerializerSettings.ContractResolver = new CamelCaseContractResolver(); });
            services.AddScoped<LiveUpdateSignalRHub>();
            services.AddScoped<SignalRDataHub>();
            services.AddScoped<OfflineSignalRDataHub>();
            services.AddScoped<SignalRParametersHub>();
            services.AddScoped<SignalRParamsGroupHub>();
            services.AddScoped<DataGridCollaborativeEditingHub>();
            services.AddScoped<SchedulerSignalRHub>();
            services.AddScoped<SignalRFlightDataHub>();
            services.AddScoped<SignalRPriorityHub>();
            services.AddScoped<MultiLineSignalRData>();
            services.AddScoped<LiveChartSignalRHub>();
            services.AddScoped<LiveDataVisualizationHub>();
            services.AddScoped<OfflineChartSignalRHub>();
            services.AddScoped<PFADataHub>();
            services.AddSingleton<ParameterDataService>();
            services.AddSingleton<SignalRDataService>();
            services.AddSingleton<OfflineSignalRDataService>();
            services.AddSingleton<SignalRParameterDataService>();
            services.AddSingleton<SignalRParamsGroupService>();
            services.AddSingleton<SignalRFlightService>();
            services.AddSingleton<SignalRPriorityService>();
            services.AddSingleton<MultilineSignalRService>();
            services.AddSingleton<LiveChartSignalRService>();
            services.AddSingleton<LiveDataVisualizationService>();
            services.AddSingleton<OfflineChartSignalRService>();
            services.AddSingleton<PFADataService>();
            services.AddSingleton<OtpService>();

            //var connection = @"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=required;";
            var connection = @"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=risk;sslmode=none;";
            services.AddDbContext<MySqlDBContext>(options => options.UseMySQL(connection));
            services.AddTransient<MySqlConnection>(_ => new MySqlConnection(Configuration["ConnectionStrings:myDb1"]));

       

            var connection2 = @"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=commondb;sslmode=required";
            services.AddDbContext<CommonDBContext>(options => options.UseMySQL(connection2));
            services.AddTransient<MySqlConnection>(_ => new MySqlConnection(Configuration["ConnectionStrings:commondb"]));

            services.AddControllers();
            services.AddHttpClient();
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.Configure<FormOptions>(o => {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [Obsolete]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();
            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
            });
            app.UseSession();
            app.UseRouting();
            app.UseCors("CorsPolicy");

            app.UseEndpoints(routes => {
                routes.MapHub<LiveUpdateSignalRHub>("/liveUpdateSignalRHub");
                routes.MapHub<SignalRDataHub>("/SignalRDataHub");
                routes.MapHub<OfflineSignalRDataHub>("/OfflineSignalRDataHub");
                routes.MapHub<DataGridCollaborativeEditingHub>("/dataGridCollaborativeEditingHub");
                routes.MapHub<SchedulerSignalRHub>("/schedulerSignalRHub");
                routes.MapHub<SignalRParamsGroupHub>("/SignalRParamsGroupHub");
                routes.MapHub<SignalRParametersHub>("/SignalRParametersHub");
                routes.MapHub<SignalRFlightDataHub>("/SignalRFlightDataHub");
                routes.MapHub<SignalRPriorityHub>("/SignalRPriorityHub");
                routes.MapHub<MultiLineSignalRData>("/MultiLineSignalRData");
                routes.MapHub<LiveChartSignalRHub>("/LiveChartSignalRHub");
                routes.MapHub<LiveDataVisualizationHub>("/LiveDataVisualizationHub");
                routes.MapHub<OfflineChartSignalRHub>("/OfflineChartSignalRHub");
                routes.MapHub<PFADataHub>("/PFADataHub");
            });
            app.UseAuthorization();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider2(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
                RequestPath = new PathString("/Resources")
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChartHub>("/chart");
                endpoints.MapHub<ChartHub>("/demo");
                endpoints.MapHub<TrendChartHub>("/trendchart");
            });
        }
    }
}