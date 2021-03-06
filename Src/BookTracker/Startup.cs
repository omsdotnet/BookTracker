﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace BookTracker
{
  public class Startup
  {
    private readonly IConfiguration _configuration;
    private readonly IHostingEnvironment _environment;
    private readonly ILogger _log;

    private bool _sslIsAvailable;

    public Startup(IConfiguration configuration, IHostingEnvironment env, ILogger<Startup> logger)
    {
      _configuration = configuration;
      _environment = env;
      _log = logger;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      _sslIsAvailable = _configuration?.GetValue<bool>("AppSettings:UseSsl") ?? false;

      //// **** VERY IMPORTANT *****
      // This is a custom extension method in Config/DataProtection.cs
      // These settings require your review to correctly configur data protection for your environment
      services.SetupDataProtection(_configuration, _environment);

      services.AddAuthorization(options =>
      {
        //https://docs.asp.net/en/latest/security/authorization/policies.html
        //** IMPORTANT ***
        //This is a custom extension method in Config/Authorization.cs
        //That is where you can review or customize or add additional authorization policies
        options.SetupAuthorizationPolicies();

      });

      //// **** IMPORTANT *****
      // This is a custom extension method in Config/CloudscribeFeatures.cs
      services.SetupDataStorage(_configuration);

      //*** Important ***
      // This is a custom extension method in Config/CloudscribeFeatures.cs
      services.SetupCloudscribeFeatures(_configuration);

      //*** Important ***
      // This is a custom extension method in Config/Localization.cs
      services.SetupLocalization();

      //*** Important ***
      // This is a custom extension method in Config/RoutingAndMvc.cs
      services.SetupMvc(_sslIsAvailable);


    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(
        IApplicationBuilder app,
        IHostingEnvironment env,
        ILoggerFactory loggerFactory,
        IOptions<cloudscribe.Core.Models.MultiTenantOptions> multiTenantOptionsAccessor,
        IOptions<RequestLocalizationOptions> localizationOptionsAccessor
        )
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseDatabaseErrorPage();
      }
      else
      {
        app.UseExceptionHandler("/oops/error");
      }
      app.UseStaticFiles();

      app.UseCloudscribeCommonStaticFiles();

      app.UseRequestLocalization(localizationOptionsAccessor.Value);

      var multiTenantOptions = multiTenantOptionsAccessor.Value;

      app.UseCloudscribeCore(
              loggerFactory,
              multiTenantOptions,
              _sslIsAvailable);


      app.UseMvc(routes =>
      {
        var useFolders = multiTenantOptions.Mode == cloudscribe.Core.Models.MultiTenantMode.FolderName;
        //*** IMPORTANT ***
        // this is in Config/RoutingAndMvc.cs
        // you can change or add routes there
        routes.UseCustomRoutes(useFolders);
      });

    }
  }
}