using kweb.Models;

using Microsoft.AspNetCore.Server.Kestrel.Core;

using System.Net;

namespace kweb.Server;

/// <summary>
/// The main Kestrel WebApplication ***ENDPOINT ROUTING*** catch-all logic that can bootstrap and proxy the logic to Azos.Wave
/// </summary>
public class WebShimEndpointRouting
{
  /// <summary>
  /// Creates and Builds a Kestral WebApplication that proxies logic through existing Azos web logic based on the supplied args and configuration
  /// </summary>
  public WebApplication BuildWebApplication(string[] args, WebShimOptions options)
  {
    var builder = getBuilder(args, options);
    builder.WebHost.ConfigureKestrel(loadKestrelConfig(options)); // TODO: create options model to pass as parameter to LoadKestrelConfig
    var app = builder.Build();

    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
      endpoints.Map("{**slug}", context =>
      {
        // **** TODO: Proxy Middleware RequestDelegate logic to Azos here ****
        return context.Response.WriteAsync(context.Request.QueryString.ToString());
      });

      endpoints.Map("/sysinfo", context =>
      {
        // **** TODO: Add System Info here ****
        return context.Response.WriteAsync("sysinfo page goes here");
      });
    });
    return app;
  }

  #region Private Methods

  // **** TODO: Replace all of the hardcoded defaults with configuration properties and/or .consts ****

  private WebApplicationBuilder getBuilder(string[] args, WebShimOptions options)
  {
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
      Args = options.AppOptions.Args ?? args,
      // **** TODO: Add IConfig and load settings below from configuration ****
      ApplicationName = options.AppOptions.ApplicationName ?? typeof(Program).Assembly.FullName,
      ContentRootPath = options.AppOptions.ContentRootPath ?? Directory.GetCurrentDirectory(),
      EnvironmentName = options.AppOptions.EnvironmentName ?? Environments.Production,
      WebRootPath = options.AppOptions.WebRootPath ?? "wwwroot"
    });
    return builder;
  }

  private Action<KestrelServerOptions> loadKestrelConfig(WebShimOptions options) // **** TODO: create options model to pass as parameter ****
  {
    return serverOptions =>
    {
      serverOptions.Limits.MaxConcurrentConnections = options.KestrelOptions.Limits.MaxConcurrentConnections ?? 1000;
      serverOptions.Limits.MaxConcurrentUpgradedConnections = options.KestrelOptions.Limits.MaxConcurrentUpgradedConnections ?? 1000;
      serverOptions.Limits.MaxRequestBodySize = options.KestrelOptions.Limits.MaxRequestBodySize ?? 100 * 1024 * 1024;
      serverOptions.Limits.MinRequestBodyDataRate = options.KestrelOptions.Limits.MinRequestBodyDataRate ?? new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
      serverOptions.Limits.MinResponseDataRate = options.KestrelOptions.Limits.MinResponseDataRate ?? new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));

      serverOptions.Limits.KeepAliveTimeout = options.KestrelOptions.Limits.KeepAliveTimeout; // Default is 30 seconds do we want to set default, min, and max?
      serverOptions.Limits.RequestHeadersTimeout = options.KestrelOptions.Limits.RequestHeadersTimeout; // Default is 30 seconds do we want to set default, min, and max?

      // HTTP
      var http = options.HttpListener;
      if (http != null)
      {
        if(http.Options == null) serverOptions.Listen(http.Address ?? IPAddress.Loopback, http.Port ?? 5105);
        else serverOptions.Listen(http.Address ?? IPAddress.Loopback, http.Port ?? 5105, http.Options);
      }

      // HTTPS
      var https = options.HttpsListener;
      if (https != null)
      {
        if (https.Options == null) serverOptions.Listen(
          https.Address ?? IPAddress.Loopback,
          https.Port ?? 5105,
          listenOptions =>
            {
              listenOptions.Protocols = HttpProtocols.Http2;
              listenOptions.UseHttps("localhost.pfx", "YourSecurePassword");
            });
        else serverOptions.Listen(https.Address ?? IPAddress.Loopback, https.Port ?? 5105, https.Options);
      }
    };
  }

  #endregion
}

