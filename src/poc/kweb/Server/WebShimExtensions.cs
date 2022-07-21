using System.Net;

using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace kweb.Server;

public static class WebShimExtensions
{
  public static Action<KestrelServerOptions> LoadKestrelConfig() // **** TODO: create options model to pass as parameter ****
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
}
