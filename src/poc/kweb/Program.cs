using kweb.Server;

namespace kweb;

public class Program
{
  public async static Task Main(string[] args)
  {
    //var app = new WebShim().BuildWebApplication(args);
    //app.Run();

    var app1 = await BuildAndStartAppAsync(args).ConfigureAwait(false);
    //var app2 = await BuildAndStartAppAsync(args).ConfigureAwait(false);

    Console.ReadKey(true);
    app1.StopAsync().Wait();
    //app2.StopAsync().Wait();
  }

  public async static Task<WebApplication> BuildAndStartAppAsync(string[] args)
  {
    var app = new WebShim().BuildWebApplication(args);
    await app.StartAsync().ConfigureAwait(false);
    return app;
  }
}
