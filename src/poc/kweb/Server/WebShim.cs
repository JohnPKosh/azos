using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

namespace kweb.Server;

/// <summary>
/// The main Kestrel WebApplication catch-all logic that can bootstrap and proxy the logic to Azos.Wave
/// </summary>
public class WebShim
{
  /// <summary>
  /// Creates and Builds a Kestral WebApplication that proxies logic through existing Azos web logic based on the supplied args and configuration
  /// </summary>
  public WebApplication BuildWebApplication(string[] args)
  {
    var builder = getBuilder(args);
    builder.WebHost.ConfigureKestrel(loadKestrelConfig()); // TODO: create options model to pass as parameter to LoadKestrelConfig
    var app = builder.Build();

    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
      endpoints.Map("{**slug}", context =>
      {
        // **** TODO: Proxy RequestDelegate logic to Azos here ****
        return context.Response.WriteAsync(context.Request.QueryString.ToString());
      });
    });
    return app;
  }

  #region Private Methods

  private WebApplicationBuilder getBuilder(string[] args)
  {
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
      Args = args,
      // **** TODO: Add IConfig and load settings below from configuration ****
      ApplicationName = typeof(Program).Assembly.FullName,
      ContentRootPath = Directory.GetCurrentDirectory(),
      EnvironmentName = Environments.Staging,
      WebRootPath = "wwwroot"
    });
    return builder;
  }

  private Action<KestrelServerOptions> loadKestrelConfig() // **** TODO: create options model to pass as parameter ****
  {
    return serverOptions =>
    {
      serverOptions.Limits.MaxConcurrentConnections = 1000;
      serverOptions.Limits.MaxConcurrentUpgradedConnections = 1000;
      serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
      serverOptions.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
      serverOptions.Limits.MinResponseDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));

      // HTTP
      serverOptions.Listen(IPAddress.Loopback, 5105);

      // HTTPS
      serverOptions.Listen(IPAddress.Loopback, 5106,
          listenOptions =>
          {
            listenOptions.Protocols = HttpProtocols.Http2;
            listenOptions.UseHttps("localhost.pfx", "YourSecurePassword");
          });

      serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
      serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
    };
  }

  #endregion
}
