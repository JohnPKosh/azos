
using Microsoft.AspNetCore.Server.Kestrel.Core;

using System.Net;

namespace kweb.Models;

public class WebShimListener
{
  public IPAddress? Address { get; set; }

  public int? Port { get; set; }

  public Action<ListenOptions>? Options { get; set; }
}