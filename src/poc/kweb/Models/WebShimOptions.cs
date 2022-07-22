
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace kweb.Models;

public class WebShimOptions
{
  public WebApplicationOptions AppOptions { get; set; } = new WebApplicationOptions();

  public KestrelServerOptions KestrelOptions { get; set; } = new KestrelServerOptions();

  public WebShimListener? HttpListener { get; set; }

  public WebShimListener? HttpsListener { get; set; }
}
