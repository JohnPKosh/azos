
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace kweb.Models;

public class WebBuilderOptions
{
  public WebApplicationOptions AppOptions { get; set; } = new WebApplicationOptions();

  public KestrelServerOptions KestrelOptions { get; set; } = new KestrelServerOptions();

  public WebListenerOptions? HttpListener { get; set; }

  public WebListenerOptions? HttpsListener { get; set; }
}
