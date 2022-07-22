using kweb.Logic;
using kweb.Models;

namespace kweb.Server;


/// <summary>
/// The main Kestrel WebApplication *** MIDDLEWARE *** sample logic that can bootstrap and proxy the logic to Azos.Wave
/// </summary>
public class WebShim_Middleware : WebShimBase
{
  /// <summary>
  /// Creates and Builds a Kestral WebApplication that proxies logic through existing Azos web logic based on the supplied args and configuration
  /// </summary>
  public WebApplication BuildWebApplication(string[] args, WebBuilderOptions options)
  {
    var builder = GetBuilder(args, options); // Get Builder
    builder.WebHost.ConfigureKestrel(LoadKestrelConfig(options)); // Configure Kestrel
    var app = builder.Build(); // Build the WebApplication
    app.UseWave(); // Add Azos Wave Middleware

    return app;
  }
}


/// <summary>
/// The main Kestrel WebApplication *** ENDPOINT ROUTING *** sample logic that can bootstrap and proxy the logic to Azos.Wave
/// <remarks>
///   Contains an additional sample "/sysinfo" system route
/// </remarks>
/// </summary>
public class WebShim_EndpointRouting : WebShimBase
{
  /// <summary>
  /// Creates and Builds a Kestral WebApplication that proxies sample logic through existing Azos web logic based on the supplied args and configuration
  /// </summary>
  public WebApplication BuildWebApplication(string[] args, WebBuilderOptions options)
  {
    var builder = GetBuilder(args, options);
    builder.WebHost.ConfigureKestrel(LoadKestrelConfig(options));
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