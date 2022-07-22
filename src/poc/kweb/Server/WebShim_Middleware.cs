using kweb.Logic;
using kweb.Models;

using Microsoft.AspNetCore.Server.Kestrel.Core;

using System.Net;

namespace kweb.Server;

/// <summary>
/// The main Kestrel WebApplication *** Middleware *** logic that can bootstrap and proxy the logic to Azos.Wave
/// </summary>
public class WebShim_Middleware : WebShimBase
{
  /// <summary>
  /// Creates and Builds a Kestral WebApplication that proxies logic through existing Azos web logic based on the supplied args and configuration
  /// </summary>
  public WebApplication BuildWebApplication(string[] args, WebBuilderOptions options)
  {
    var builder = getBuilder(args, options); // Get Builder
    builder.WebHost.ConfigureKestrel(loadKestrelConfig(options)); // Configure Kestrel
    var app = builder.Build(); // Build the WebApplication
    app.UseWave(); // Add Azos Wave Middleware

    return app;
  }
}
