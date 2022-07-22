using System.Net;

using kweb.Models;

using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace kweb.Server;

public class WebShimBase
{
  // **** TODO: Externalize and replace all of the hardcoded defaults with configuration properties and/or .consts ****
  #region .consts

  public const bool KO_RESP_HDR_COMPRESSION = true;
  public const bool KO_ADD_SERVER_HDR = true;
  public const int KO_MAX_CONCURRENT_CONNECTIONS = 1_000;
  public const int KO_MAX_UPGRADED_CONNECTIONS = 1_000;
  public const long KO_MAX_REQ_BODY_SIZE = 100 * 1_024 * 1_024;

  public const int KO_MIN_REQ_BPS = 100;
  public static readonly TimeSpan KO_MIN_REQ_TS = TimeSpan.FromSeconds(10);
  public static readonly MinDataRate KO_MIN_REQ_BODY_DATA_RATE = new MinDataRate(bytesPerSecond: KO_MIN_REQ_BPS, gracePeriod: KO_MIN_REQ_TS);

  public const int KO_MIN_RSP_BPS = 100;
  public static readonly TimeSpan KO_MIN_RSP_TS = TimeSpan.FromSeconds(10);
  public static readonly MinDataRate KO_MIN_RSP_DATA_RATE = new MinDataRate(bytesPerSecond: KO_MIN_RSP_BPS, gracePeriod: KO_MIN_RSP_TS);

  public static readonly IPAddress KO_DEFAULT_IP = IPAddress.Loopback;
  public const int KO_HTTP_DEFAULT_PORT = 5105;
  public const int KO_HTTPS_DEFAULT_PORT = 5106;
  public const string KO_CERT_FILENAME = "localhost.pfx";
  public const string KO_CERT_PWD = "YourSecurePassword";

  #endregion

  #region Protected

  protected WebApplicationBuilder GetBuilder(string[] args, WebBuilderOptions options)
  {
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
      Args = options.AppOptions.Args ?? args,

      // **** TODO: Add IConfig and load settings below from configuration ****
      ApplicationName = options.AppOptions.ApplicationName ?? typeof(Program).Assembly.FullName,
      ContentRootPath = options.AppOptions.ContentRootPath ?? Directory.GetCurrentDirectory(),
      EnvironmentName = options.AppOptions.EnvironmentName ?? Environments.Production,
      WebRootPath = options.AppOptions.WebRootPath ?? "wwwroot"
    });
    return builder;
  }

  protected Action<KestrelServerOptions> LoadKestrelConfig(WebBuilderOptions options) // **** TODO: create options model to pass as parameter ****
  {
    return serverOptions =>
    {
      serverOptions.AllowResponseHeaderCompression = KO_RESP_HDR_COMPRESSION; // default true
      serverOptions.AddServerHeader = KO_ADD_SERVER_HDR; // default true
      serverOptions.Limits.MaxConcurrentConnections = options.KestrelOptions.Limits.MaxConcurrentConnections ?? KO_MAX_CONCURRENT_CONNECTIONS;
      serverOptions.Limits.MaxConcurrentUpgradedConnections = options.KestrelOptions.Limits.MaxConcurrentUpgradedConnections ?? KO_MAX_UPGRADED_CONNECTIONS;
      serverOptions.Limits.MaxRequestBodySize = options.KestrelOptions.Limits.MaxRequestBodySize ?? KO_MAX_REQ_BODY_SIZE;
      serverOptions.Limits.MinRequestBodyDataRate = options.KestrelOptions.Limits.MinRequestBodyDataRate ?? KO_MIN_REQ_BODY_DATA_RATE;
      serverOptions.Limits.MinResponseDataRate = options.KestrelOptions.Limits.MinResponseDataRate ?? KO_MIN_RSP_DATA_RATE;
      serverOptions.Limits.KeepAliveTimeout = options.KestrelOptions.Limits.KeepAliveTimeout; // Default is 30 seconds do we want to set default, min, and max?
      serverOptions.Limits.RequestHeadersTimeout = options.KestrelOptions.Limits.RequestHeadersTimeout; // Default is 30 seconds do we want to set default, min, and max?

      // HTTP
      var http = options.HttpListener;
      if (http != null)
      {
        if (http.Options == null) serverOptions.Listen(http.Address ?? KO_DEFAULT_IP, http.Port ?? KO_HTTP_DEFAULT_PORT);
        else serverOptions.Listen(http.Address ?? KO_DEFAULT_IP, http.Port ?? KO_HTTP_DEFAULT_PORT, http.Options);
      }

      // HTTPS
      var https = options.HttpsListener;
      if (https != null)
      {
        if (https.Options == null) serverOptions.Listen(
          https.Address ?? KO_DEFAULT_IP,
          https.Port ?? KO_HTTPS_DEFAULT_PORT,
          listenOptions =>
            {
              listenOptions.Protocols = HttpProtocols.Http2;
              listenOptions.UseHttps(KO_CERT_FILENAME, KO_CERT_PWD);
            });
        else serverOptions.Listen(https.Address ?? KO_DEFAULT_IP, https.Port ?? KO_HTTPS_DEFAULT_PORT, https.Options);
      }
    };
  }

  #endregion
}