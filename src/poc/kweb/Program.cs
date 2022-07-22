using kweb.Models;
using kweb.Server;

namespace kweb;

public class Program
{
  public async static Task Main(string[] args)
  {
    // Create and Start *** MIDDLEWARE *** WebApplication
    var app1Options = new WebBuilderOptions();
    var app1 = await BuildAndStartApp_Middleware_Async(args, app1Options).ConfigureAwait(false);


    // Create and Start an *** ENDPOINT ROUTING *** WebApplication
    var app2Options = new WebBuilderOptions()
    {
      HttpListener = new WebListenerOptions { Port = 6200},
      HttpsListener = new WebListenerOptions { Port = 6201}
    };
    var app2 = await BuildAndStartApp_EndpointRouting_Async(args, app2Options).ConfigureAwait(false);

    // Wait around for any key to exit
    Console.ReadKey(true);

    // Stop Web App
    app1.StopAsync().Wait();
    app2.StopAsync().Wait();
  }

  #region Private Methods

  private async static Task<WebApplication> BuildAndStartApp_Middleware_Async(string[] args, WebBuilderOptions options)
  {
    var app = new WebShim_Middleware().BuildWebApplication(args, options);
    await app.StartAsync().ConfigureAwait(false);
    return app;
  }


  private async static Task<WebApplication> BuildAndStartApp_EndpointRouting_Async(string[] args, WebBuilderOptions options)
  {
    var app = new WebShim_EndpointRouting().BuildWebApplication(args, options);
    await app.StartAsync().ConfigureAwait(false);
    return app;
  }

  #endregion
}
