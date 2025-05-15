using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
//using IGeekFan.AspNetCore.Knife4jUI;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Swagger;

namespace DB2VM
{
    public class ContainerChecker
    {
        public static bool IsRunningInDocker()
        {
            // 檢查 /.dockerenv 檔案是否存在
            if (File.Exists("/.dockerenv"))
            {
                return true;
            }

            // 檢查 /proc/1/cgroup 檔案的內容
            if (File.Exists("/proc/1/cgroup"))
            {
                string[] lines = File.ReadAllLines("/proc/1/cgroup");
                foreach (string line in lines)
                {
                    if (line.Contains("docker") || line.Contains("lxc") || line.Contains("kubepods"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        //static public void UpdateAppSettings()
        //{
        //    // 判斷是否在 Docker 中運行
        //    bool isInDocker = IsRunningInDocker();

        //    // 開啟配置檔案
        //    Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        //    // 更新設定值
        //    string newValue = isInDocker ? "127.0.0.1" : "127.0.0.1";
        //    config.AppSettings.Settings["IP"].Value = newValue;
        //    config.AppSettings.Settings["server"].Value = newValue;
        //    config.AppSettings.Settings["Server"].Value = newValue;
        //    config.AppSettings.Settings["VM_Server"].Value = newValue;
        //    config.AppSettings.Settings["device_Server"].Value = newValue;

        //    // 儲存配置檔案
        //    config.Save(ConfigurationSaveMode.Modified);

        //    // 刷新 appSettings 節，使修改立即生效
        //    System.Configuration.ConfigurationManager.RefreshSection("appSettings");
        //}
    }

    public class Startup
    {
        private static string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private readonly IWebHostEnvironment _environment;
        //public static H_Pannel_lib.UDP_Class uDP_Class;
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _environment = environment;

            //ContainerChecker.UpdateAppSettings();
            //if (_environment.IsDevelopment())
            //{
            //    uDP_Class = new UDP_Class("0.0.0.0", 29500);
            //    Console.WriteLine("DEBUG模式");
            //}
            //else
            //{
            //    uDP_Class = new UDP_Class("0.0.0.0", 29600);
            //    Console.WriteLine("非DEBUG模式");
            //}
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //添加分布式內存緩存以支持 Session
            services.AddDistributedMemoryCache();
            //配置 Session
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(5); // Session 過期時間
                options.Cookie.HttpOnly = true; // 只允許 HTTP 訪問（提高安全性）
                options.Cookie.IsEssential = true; // 確保 Session Cookie 對於 GDPR 是必需的
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                     builder => builder
                         .AllowAnyOrigin()
                         .AllowAnyMethod()
                         .AllowAnyHeader()
                         .SetIsOriginAllowed(_ => true)
                         .WithExposedHeaders("Location")
                 );

            });


            services.AddControllers();
            services.AddSignalR();

            services.AddSwaggerGen(c =>
            {
                // API 服務簡介
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Pharmacy Sysytem API",
                    Description = "Pharmacy Sysytem API",
                    License = new OpenApiLicense { Name = "鴻森智能科技有限公司 版權所有" }
                });
                c.CustomOperationIds(apiDesc =>
                {
                    var controllerAction = apiDesc.ActionDescriptor as ControllerActionDescriptor;
                    return controllerAction.ControllerName + "-" + controllerAction.ActionName;
                });

                // 讀取 XML 檔案產生 API 說明
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                //var xmlFile_HIS_DB_Lib = $"HIS_DB_Lib.xml";
                //var xmlPath_HIS_DB_Lib = Path.Combine(AppContext.BaseDirectory, xmlFile_HIS_DB_Lib);

                //c.IncludeXmlComments(xmlPath);
                //c.IncludeXmlComments(xmlPath_HIS_DB_Lib);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(builder =>
            {
                builder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true).AllowCredentials();
            });
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowSpecificOrigin");

            app.UseAuthorization();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            // 啟用 Session 中間件
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                //endpoints.MapHub<ChatHub>("/chatHub").RequireCors("AllowSpecificOrigin"); // ✅ 確保 SignalR 也允許 CORS

            });
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                //c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            //app.UseKnife4UI(c =>
            //{
            //    c.RoutePrefix = string.Empty;
            //    c.SwaggerEndpoint($"/swagger/v1/swagger.json", "h.swagger.webapi v1");
            //});
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapSwagger("{documentName}/swagger.json");
            });


        }
    }
}
