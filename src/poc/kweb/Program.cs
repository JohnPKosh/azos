using kweb.Models;
using kweb.Server;

namespace kweb;

public class Program
{
  public async static Task Main(string[] args)
  {
    /* This is a primitive sample that starts two WebApplications using Kestrel:
          - The first sample uses MIDDLEWARE pipeline to write response using the middleware logic.
          - The second sample uses ENDPOINT ROUTING with a catch-all route logic.
    */

    // #1. Create and Start MIDDLEWARE WebApplication
    var middlewareOptions = new WebBuilderOptions();
    var middlewareApp = await BuildAndStartApp_Middleware_Async(args, middlewareOptions).ConfigureAwait(false);


    // #2. Create and Start ENDPOINT ROUTING WebApplication
    var endpointRoutingOptions = new WebBuilderOptions()
    {
      HttpListener = new WebListenerOptions { Port = 6200},
      HttpsListener = new WebListenerOptions { Port = 6201}
    };
    var endpointRoutingApp = await BuildAndStartApp_EndpointRouting_Async(args, endpointRoutingOptions).ConfigureAwait(false);

    /* #3. Test the two Web Applications:

      - MIDDLEWARE URLs:

          https://localhost:5106/blah/bladddh?demo=true
          http://localhost:5105/blah/bladddh?demo=true

      - ENDPOINT ROUTING URLs:

          https://localhost:6201/?tezt/demo=app2
          http://localhost:6200/?tezt/demo=app2
          https://localhost:6201/sysinfo
          http://localhost:6200/sysinfo
    */

    // #4. Press any key to exit
    Console.ReadKey(true);

    // #5. Stop Web Apps
    middlewareApp.StopAsync().Wait();
    endpointRoutingApp.StopAsync().Wait();
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
