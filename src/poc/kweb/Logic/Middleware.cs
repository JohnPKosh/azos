namespace kweb.Logic;

/// <summary>
/// Custom Middleware extension methods (e.g. "UseWave")
/// </summary>
public static class MiddlewareExtensions
{
  /// <summary>
  /// Adds the ShimMiddleware terminal middleware used to pass logic to Azos.Wave logic
  /// </summary>
  /// <param name="builder"></param>
  /// <returns></returns>
  public static IApplicationBuilder UseWave(this IApplicationBuilder builder)
  {
    return builder.UseMiddleware<WaveProxyMiddleware>();
  }
}

/// <summary>
/// Terminal middleware used to pass logic to Azos.Wave logic
/// </summary>
public class WaveProxyMiddleware
{
  private readonly RequestDelegate _next;

  public WaveProxyMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    // **** TODO: Proxy Middleware RequestDelegate logic to Azos here ****
    await context.Response.WriteAsync(context.Request.QueryString.ToString());
  }
}