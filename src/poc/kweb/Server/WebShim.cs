namespace kweb.Server;

public class WebShim
{
  public WebApplication BuildWebApplication(string[] args)
  {
    var builder = getBuilder(args);
    builder.WebHost.ConfigureKestrel(WebShimExtensions.LoadKestrelConfig()); // TODO: create options model to pass as parameter to LoadKestrelConfig
    var app = builder.Build();

    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
      endpoints.Map("{**slug}", context =>
      {
        // Do work here!
        return context.Response.WriteAsync(context.Request.QueryString.ToString());
      });
    });
    return app;
  }

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
}
