using System.Net;

using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace kweb.Models;

/// <summary>
/// Options model used to optionally configure a WebApplication
/// with WebApplicationOptions, KestrelServerOptions, WebListenerOptions for HTTP and HTTPS.
/// </summary>
public class WebBuilderOptions
{
  public WebApplicationOptions AppOptions { get; set; } = new WebApplicationOptions();

  public KestrelServerOptions KestrelOptions { get; set; } = new KestrelServerOptions();

  public WebListenerOptions? HttpListener { get; set; }

  public WebListenerOptions? HttpsListener { get; set; }
}

/// <summary>
/// Option model with IPAddress, Port, and ListenOptions props.
/// </summary>
public class WebListenerOptions
{
  public IPAddress? Address { get; set; }

  public int? Port { get; set; }

  public Action<ListenOptions>? Options { get; set; }
}