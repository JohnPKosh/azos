using kweb.Models;

using Microsoft.AspNetCore.Server.Kestrel.Core;

using System.Net;

namespace kweb.Server;

/// <summary>
/// The main Kestrel WebApplication ***ENDPOINT ROUTING*** catch-all logic that can bootstrap and proxy the logic to Azos.Wave
/// </summary>
public class WebShim_EndpointRouting : WebShimBase
{
  /// <summary>
  /// Creates and Builds a Kestral WebApplication that proxies logic through existing Azos web logic based on the supplied args and configuration
  /// </summary>
  public WebApplication BuildWebApplication(string[] args, WebBuilderOptions options)
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

}

