using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevTreks.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevTreks
{
    /// <summary>
    ///Purpose:		Configure the web app and start MVC web page
    ///             delivery.
    ///Author:		www.devtreks.org
    ///Date:		2018, September
    ///References:	www.devtreks.org
    /// </summary>
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            //configuring user secrets 
            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                //210 changes
                //https://github.com/aspnet/Announcements/issues/223
                builder.AddUserSecrets<Startup>();
            }
            else
            {
                //not used
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            ContentURI = new DevTreks.Data.ContentURI();
            //set the webroot full file path: C:\\DevTreks\\src\\DevTreks\\wwwroot
            DefaultRootFullFilePath = string.Concat(env.WebRootPath, "\\");

            //vs start code
            //Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        //set config and httpcontext settings
        DevTreks.Data.ContentURI ContentURI { get; set; }
        private static string DefaultRootFullFilePath { get; set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(
                   Configuration.GetConnectionString("DebugConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //this will be passed to Controller constructor and used to 
            //construct services and repositories 
            //inject config settings in controller and pass to views and repositories

            services.Configure<DevTreks.Data.ContentURI>(ContentURI =>
            {
                //azure is debugged by commenting in and out the azure for localhost:5000 appsettings
                string connection = string.Empty;
                string path = string.Empty;
                //comment out if secrets are not being used
                //connection = sSecretConnection;
                //comment out if secrets are being used
                connection = Configuration["ConnectionStrings:DebugConnection"];
                ContentURI.URIDataManager.DefaultConnection = connection;
                //comment out if secrets are not being used
                //connection = Configuration["DevTreksLocalStorage"];
                //comment out if secrets are being used
                connection = Configuration["ConnectionStrings:DebugStorageConnection"];
                ContentURI.URIDataManager.StorageConnection = connection;

                ContentURI.URIDataManager.DefaultRootFullFilePath
                    = DefaultRootFullFilePath;
                path = Configuration["DebugPaths:DefaultRootWebStoragePath"];
                ContentURI.URIDataManager.DefaultRootWebStoragePath = path;
                path = Configuration["DebugPaths:DefaultWebDomain"];
                ContentURI.URIDataManager.DefaultWebDomain = path;
                //210 changed to Extensions subfolder in devtreks.exe path
                ContentURI.URIDataManager.ExtensionsPath
                    = ContentURI.URIDataManager.DefaultRootFullFilePath.Replace("wwwroot\\", "wwwroot\\Extensions");
                //2.0.2 added appsetting to eliminate calls to GetPlatformType()
                ContentURI.URIDataManager.PlatformType
                    = Data.Helpers.FileStorageIO.GetPlatformType(ContentURI.URIDataManager.DefaultRootWebStoragePath);
                path = Configuration["Site:FileSizeValidation"];
                ContentURI.URIDataManager.FileSizeValidation = path;
                path = Configuration["Site:FileSizeDBStorageValidation"];
                ContentURI.URIDataManager.FileSizeDBStorageValidation = path;
                path = Configuration["Site:PageSize"];
                ContentURI.URIDataManager.PageSize
                = DevTreks.Data.Helpers.GeneralHelpers.ConvertStringToInt(path);
                path = Configuration["Site:PageSizeEdits"];
                ContentURI.URIDataManager.PageSizeEdits = path;
                path = Configuration["Site:RExecutable"];
                ContentURI.URIDataManager.RExecutable = path;
                path = Configuration["Site:PyExecutable"];
                ContentURI.URIDataManager.PyExecutable = path;
                path = Configuration["Site:JuliaExecutable"];
                ContentURI.URIDataManager.JuliaExecutable = path;
                path = Configuration["Site:HostFeeRate"];
                ContentURI.URIDataManager.HostFeeRate = path;
                path = Configuration["URINames:ResourceURIName"];
                ContentURI.URIDataManager.ResourceURIName = path;
                path = Configuration["URINames:ContentURIName"];
                ContentURI.URIDataManager.ContentURIName = path;
                path = Configuration["URINames:TempDocsURIName"];
                ContentURI.URIDataManager.TempDocsURIName = path;

            });
        }
        public void ConfigureProductionServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(
                   Configuration.GetConnectionString("ReleaseConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //this will be passed to Controller constructor and used to 
            //construct services and repositories 
            //inject config settings in controller and pass to views and repositories

            services.Configure<DevTreks.Data.ContentURI>(ContentURI =>
            {
                //azure is debugged by commenting in and out the azure for localhost:5000 appsettings
                string connection = string.Empty;
                string path = string.Empty;
                //comment out if secrets are not being used
                //connection = sSecretConnection;
                //comment out if secrets are being used
                connection = Configuration["ConnectionStrings:ReleaseConnection"];
                ContentURI.URIDataManager.DefaultConnection = connection;
                //comment out if secrets are not being used
                //connection = Configuration["DevTreksLocalStorage"];
                //comment out if secrets are being used
                connection = Configuration["ConnectionStrings:ReleaseStorageConnection"];
                ContentURI.URIDataManager.StorageConnection = connection;

                ContentURI.URIDataManager.DefaultRootFullFilePath
                    = DefaultRootFullFilePath;
                path = Configuration["ReleasePaths:DefaultRootWebStoragePath"];
                ContentURI.URIDataManager.DefaultRootWebStoragePath = path;
                path = Configuration["ReleasePaths:DefaultWebDomain"];
                ContentURI.URIDataManager.DefaultWebDomain = path;
                //210 changed to Extensions subfolder in devtreks.exe path
                ContentURI.URIDataManager.ExtensionsPath
                    = ContentURI.URIDataManager.DefaultRootFullFilePath.Replace("wwwroot\\", "wwwroot\\Extensions");
                //2.0.2 added appsetting to eliminate calls to GetPlatformType()
                ContentURI.URIDataManager.PlatformType
                    = Data.Helpers.FileStorageIO.GetPlatformType(ContentURI.URIDataManager.DefaultRootWebStoragePath);
                path = Configuration["Site:FileSizeValidation"];
                ContentURI.URIDataManager.FileSizeValidation = path;
                path = Configuration["Site:FileSizeDBStorageValidation"];
                ContentURI.URIDataManager.FileSizeDBStorageValidation = path;
                path = Configuration["Site:PageSize"];
                ContentURI.URIDataManager.PageSize
                = DevTreks.Data.Helpers.GeneralHelpers.ConvertStringToInt(path);
                path = Configuration["Site:PageSizeEdits"];
                ContentURI.URIDataManager.PageSizeEdits = path;
                path = Configuration["Site:RExecutable"];
                ContentURI.URIDataManager.RExecutable = path;
                path = Configuration["Site:PyExecutable"];
                ContentURI.URIDataManager.PyExecutable = path;
                path = Configuration["Site:JuliaExecutable"];
                ContentURI.URIDataManager.JuliaExecutable = path;
                path = Configuration["Site:HostFeeRate"];
                ContentURI.URIDataManager.HostFeeRate = path;
                path = Configuration["URINames:ResourceURIName"];
                ContentURI.URIDataManager.ResourceURIName = path;
                path = Configuration["URINames:ContentURIName"];
                ContentURI.URIDataManager.ContentURIName = path;
                path = Configuration["URINames:TempDocsURIName"];
                ContentURI.URIDataManager.TempDocsURIName = path;

            });
        }
        //public void ConfigureDevelopmentServices(IServiceCollection services)
        //{
        //    services.Configure<CookiePolicyOptions>(options =>
        //    {
        //        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        //        options.CheckConsentNeeded = context => true;
        //        options.MinimumSameSitePolicy = SameSiteMode.None;
        //    });
        //    string sConnect = Configuration.GetConnectionString("DebugConnection");
        //    services.AddDbContext<ApplicationDbContext>(options =>
        //       options.UseSqlServer(sConnect));
        //    services.AddDefaultIdentity<IdentityUser>()
        //        .AddEntityFrameworkStores<ApplicationDbContext>();

        //    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

        //    //this will be passed to Controller constructor and used to 
        //    //construct services and repositories 
        //    //inject config settings in controller and pass to views and repositories

        //    //azure is debugged by commenting in and out the azure for localhost:5001 appsettings
        //    string path = string.Empty;
        //    //comment out if secrets are not being used
        //    //connection = sSecretConnection;
        //    //comment out if secrets are being used
        //    ContentURI.URIDataManager.DefaultConnection = sConnect;
        //    //comment out if secrets are not being used
        //    //connection = Configuration["DevTreksLocalStorage"];
        //    //comment out if secrets are being used
        //    sConnect = Configuration.GetConnectionString("DebugStorageConnection");
        //    ContentURI.URIDataManager.StorageConnection = sConnect;

        //    ContentURI.URIDataManager.DefaultRootFullFilePath
        //        = DefaultRootFullFilePath;
        //    path = Configuration["DebugPaths:DefaultRootWebStoragePath"];
        //    ContentURI.URIDataManager.DefaultRootWebStoragePath = path;
        //    path = Configuration["DebugPaths:DefaultWebDomain"];
        //    ContentURI.URIDataManager.DefaultWebDomain = path;
        //    //210 changed to Extensions subfolder in devtreks.exe path
        //    ContentURI.URIDataManager.ExtensionsPath
        //        = ContentURI.URIDataManager.DefaultRootFullFilePath.Replace("wwwroot\\", "wwwroot\\Extensions");
        //    //2.0.2 added appsetting to eliminate calls to GetPlatformType()
        //    ContentURI.URIDataManager.PlatformType
        //        = Data.Helpers.FileStorageIO.GetPlatformType(ContentURI.URIDataManager.DefaultRootWebStoragePath);
        //    path = Configuration["Site:FileSizeValidation"];
        //    ContentURI.URIDataManager.FileSizeValidation = path;
        //    path = Configuration["Site:FileSizeDBStorageValidation"];
        //    ContentURI.URIDataManager.FileSizeDBStorageValidation = path;
        //    path = Configuration["Site:PageSize"];
        //    ContentURI.URIDataManager.PageSize
        //    = DevTreks.Data.Helpers.GeneralHelpers.ConvertStringToInt(path);
        //    path = Configuration["Site:PageSizeEdits"];
        //    ContentURI.URIDataManager.PageSizeEdits = path;
        //    path = Configuration["Site:RExecutable"];
        //    ContentURI.URIDataManager.RExecutable = path;
        //    path = Configuration["Site:PyExecutable"];
        //    ContentURI.URIDataManager.PyExecutable = path;
        //    path = Configuration["Site:JuliaExecutable"];
        //    ContentURI.URIDataManager.JuliaExecutable = path;
        //    path = Configuration["Site:HostFeeRate"];
        //    ContentURI.URIDataManager.HostFeeRate = path;
        //    path = Configuration["URINames:ResourceURIName"];
        //    ContentURI.URIDataManager.ResourceURIName = path;
        //    path = Configuration["URINames:ContentURIName"];
        //    ContentURI.URIDataManager.ContentURIName = path;
        //    path = Configuration["URINames:TempDocsURIName"];
        //    ContentURI.URIDataManager.TempDocsURIName = path;

        //    services.Configure<DevTreks.Data.ContentURI>(ContentURI =>
        //    {
        //        path = string.Empty;
        //    }
        //    );
        //        //services.Configure<DevTreks.Data.ContentURI>(ContentURI =>
        //        //{
        //        ////azure is debugged by commenting in and out the azure for localhost:5001 appsettings
        //        //string path = string.Empty;
        //        ////comment out if secrets are not being used
        //        ////connection = sSecretConnection;
        //        ////comment out if secrets are being used
        //        //ContentURI.URIDataManager.DefaultConnection = sConnect;
        //        ////comment out if secrets are not being used
        //        ////connection = Configuration["DevTreksLocalStorage"];
        //        ////comment out if secrets are being used
        //        //sConnect = Configuration.GetConnectionString("DebugStorageConnection");
        //        //ContentURI.URIDataManager.StorageConnection = sConnect;

        //        //ContentURI.URIDataManager.DefaultRootFullFilePath
        //        //    = DefaultRootFullFilePath;
        //        //path = Configuration["DebugPaths:DefaultRootWebStoragePath"];
        //        //ContentURI.URIDataManager.DefaultRootWebStoragePath = path;
        //        //path = Configuration["DebugPaths:DefaultWebDomain"];
        //        //ContentURI.URIDataManager.DefaultWebDomain = path;
        //        ////210 changed to Extensions subfolder in devtreks.exe path
        //        //ContentURI.URIDataManager.ExtensionsPath
        //        //    = ContentURI.URIDataManager.DefaultRootFullFilePath.Replace("wwwroot\\", "wwwroot\\Extensions");
        //        ////2.0.2 added appsetting to eliminate calls to GetPlatformType()
        //        //ContentURI.URIDataManager.PlatformType
        //        //    = Data.Helpers.FileStorageIO.GetPlatformType(ContentURI.URIDataManager.DefaultRootWebStoragePath);
        //        //path = Configuration["Site:FileSizeValidation"];
        //        //ContentURI.URIDataManager.FileSizeValidation = path;
        //        //path = Configuration["Site:FileSizeDBStorageValidation"];
        //        //ContentURI.URIDataManager.FileSizeDBStorageValidation = path;
        //        //path = Configuration["Site:PageSize"];
        //        //ContentURI.URIDataManager.PageSize
        //        //= DevTreks.Data.Helpers.GeneralHelpers.ConvertStringToInt(path);
        //        //path = Configuration["Site:PageSizeEdits"];
        //        //ContentURI.URIDataManager.PageSizeEdits = path;
        //        //path = Configuration["Site:RExecutable"];
        //        //ContentURI.URIDataManager.RExecutable = path;
        //        //path = Configuration["Site:PyExecutable"];
        //        //ContentURI.URIDataManager.PyExecutable = path;
        //        //path = Configuration["Site:JuliaExecutable"];
        //        //ContentURI.URIDataManager.JuliaExecutable = path;
        //        //path = Configuration["Site:HostFeeRate"];
        //        //ContentURI.URIDataManager.HostFeeRate = path;
        //        //path = Configuration["URINames:ResourceURIName"];
        //        //ContentURI.URIDataManager.ResourceURIName = path;
        //        //path = Configuration["URINames:ContentURIName"];
        //        //ContentURI.URIDataManager.ContentURIName = path;
        //        //path = Configuration["URINames:TempDocsURIName"];
        //        //ContentURI.URIDataManager.TempDocsURIName = path;

        //        //});
        //}
        //public void ConfigureProductionServices(IServiceCollection services)
        //{
        //    services.Configure<CookiePolicyOptions>(options =>
        //    {
        //        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        //        options.CheckConsentNeeded = context => true;
        //        options.MinimumSameSitePolicy = SameSiteMode.None;
        //    });
        //    string sConnect = Configuration.GetConnectionString("ReleaseConnection");
        //    services.AddDbContext<ApplicationDbContext>(options =>
        //       options.UseSqlServer(sConnect));
        //    services.AddDefaultIdentity<IdentityUser>()
        //        .AddEntityFrameworkStores<ApplicationDbContext>();

        //    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        //    //this will be passed to Controller constructor and used to 
        //    //construct services and repositories 
        //    //inject config settings in controller and pass to views and repositories
        //    services.Configure<DevTreks.Data.ContentURI>(ContentURI =>
        //    {
        //        string path = string.Empty;
        //        //azure and web server use release paths
        //        //comment out if secrets are not being used
        //        //connection = sSecretConnection;
        //        //comment out if secrets are being used
        //        ContentURI.URIDataManager.DefaultConnection = sConnect;
        //        //comment out if secrets are not being used
        //        //connection = Configuration["DevTreksLocalStorage"];
        //        //comment out if secrets are being used
        //        sConnect = Configuration.GetConnectionString("ReleaseStorageConnection");
        //        ContentURI.URIDataManager.StorageConnection = sConnect;

        //        ContentURI.URIDataManager.DefaultRootFullFilePath
        //            = DefaultRootFullFilePath;
        //        //getplatformtype expects this to be string empty to debug azure on localhost
        //        path = Configuration["ReleasePaths:DefaultRootWebStoragePath"];
        //        ContentURI.URIDataManager.DefaultRootWebStoragePath = path;
        //        path = Configuration["ReleasePaths:DefaultWebDomain"];
        //        ContentURI.URIDataManager.DefaultWebDomain = path;
        //        ContentURI.URIDataManager.ExtensionsPath
        //            = ContentURI.URIDataManager.DefaultRootFullFilePath.Replace("DevTreks\\wwwroot", "wwwroot\\Extensions");
        //        //2.0.2 added appsetting to eliminate calls to GetPlatformType()
        //        ContentURI.URIDataManager.PlatformType
        //            = Data.Helpers.FileStorageIO.GetPlatformType(ContentURI.URIDataManager.DefaultRootWebStoragePath);
        //        path = Configuration["Site:FileSizeValidation"];
        //        ContentURI.URIDataManager.FileSizeValidation = path;
        //        path = Configuration["Site:FileSizeDBStorageValidation"];
        //        ContentURI.URIDataManager.FileSizeDBStorageValidation = path;
        //        path = Configuration["Site:PageSize"];
        //        ContentURI.URIDataManager.PageSize
        //            = DevTreks.Data.Helpers.GeneralHelpers.ConvertStringToInt(path);
        //        path = Configuration["Site:PageSizeEdits"];
        //        ContentURI.URIDataManager.PageSizeEdits = path;
        //        path = Configuration["Site:RExecutable"];
        //        ContentURI.URIDataManager.RExecutable = path;
        //        path = Configuration["Site:PyExecutable"];
        //        ContentURI.URIDataManager.PyExecutable = path;
        //        path = Configuration["Site:JuliaExecutable"];
        //        ContentURI.URIDataManager.JuliaExecutable = path;
        //        path = Configuration["Site:HostFeeRate"];
        //        ContentURI.URIDataManager.HostFeeRate = path;
        //        path = Configuration["URINames:ResourceURIName"];
        //        ContentURI.URIDataManager.ResourceURIName = path;
        //        path = Configuration["URINames:ContentURIName"];
        //        ContentURI.URIDataManager.ContentURIName = path;
        //        path = Configuration["URINames:TempDocsURIName"];
        //        ContentURI.URIDataManager.TempDocsURIName = path;
        //    });

        //}
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            ILoggerFactory loggerFactory)
        {

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // errors are displayed on html content pages -don't use a separate page
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error"); 
                
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                //route name
                name: "Default",
                //{*contenturipattern} is a variable route
                template: "{controller=Home}/{action=Index}/{*contenturipattern}"
                );
            });
        }
        
    }
}
