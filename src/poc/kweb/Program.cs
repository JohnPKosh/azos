using kweb.Models;
using kweb.Server;

namespace kweb;

public class Program
{
  public async static Task Main(string[] args)
  {
    // Create and Start First WebApplication
    var app1Options = new WebShimOptions();
    var app1 = await BuildAndStartAppAsync(args, app1Options).ConfigureAwait(false);

    // Create and Start Second WebApplication
    var app2Options = new WebShimOptions()
    {
      HttpListener = new WebShimListener { Port = 6200},
      HttpsListener = new WebShimListener { Port = 6201}
    };
    var app2 = await BuildAndStartAppAsync(args, app2Options).ConfigureAwait(false);

    // Wait around to exit
    Console.ReadKey(true);

    // Stop Web App
    app1.StopAsync().Wait();
    app2.StopAsync().Wait();

    //app1.Run(async context =>
    //{
    //  await context.Response.WriteAsync("Hello Dear Readers!");
    //});
  }

  public async static Task<WebApplication> BuildAndStartAppAsync(string[] args, WebShimOptions options)
  {
    var app = new WebShim().BuildWebApplication(args, options);
    await app.StartAsync().ConfigureAwait(false);
    return app;
  }
}
