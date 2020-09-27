namespace ruttmann.vita.api.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.DependencyInjection;

  [Route("api/v1/[controller]")]
	public class TrackController : Controller
	{
		/// <summary>
		/// Get the collection of vita entries for the active code.
		/// </summary>
		/// <returns>a vita collection</returns>
    [HttpPost]
		[Authorize]
    public IActionResult Post([FromBody]UrlTrackRequest value)
    {
      var trackingService = this.HttpContext.RequestServices.GetRequiredService<ITrackingService>();
      string remoteIp = GetRemoteIp();

      var session = trackingService.GetSession(
        this.HttpContext.Request.Headers["Code"].Single(),
        this.HttpContext.Request.Headers["SessionKey"].Single(),
        GetRemoteIp());

      var trackEvent = new UrlTrackingEvent(value.Url, value.Topic, value.Duration);
      session.RecordUrl(trackEvent);

      return StatusCode(200);
    }

    /// <summary>
    /// Get the collection of vita entries for the active code.
    /// </summary>
    /// <returns>a vita collection</returns>
    [HttpPost("link")]
		[Authorize]
    public IActionResult PostClickedLink([FromBody]TrackLinkClickRequest value)
		{
			var trackingService = this.HttpContext.RequestServices.GetRequiredService<ITrackingService>();
      var session = trackingService.GetSession(
        this.HttpContext.Request.Headers["Code"].Single(),
        this.HttpContext.Request.Headers["SessionKey"].Single(),
        GetRemoteIp());

      session.RecordLinkClick(value.Url);

			return StatusCode(200);
    }

    /// <summary>
    /// Get the collection of vita entries for the active code.
    /// </summary>
    /// <returns>a vita collection</returns>
    [HttpPost("topics")]
		[Authorize]
    public IActionResult PostTopics([FromBody]TrackTopicsRequest value)
		{
			var trackingService = this.HttpContext.RequestServices.GetRequiredService<ITrackingService>();
      var session = trackingService.GetSession(
        this.HttpContext.Request.Headers["Code"].Single(),
        this.HttpContext.Request.Headers["SessionKey"].Single(),
        GetRemoteIp());

			var remoteIp = this.GetRemoteIp();
			var trackEvent = new TrackTopicsEvent(
				value.Url,
				value.Duration,
				value.Topics);

      session.RecordTopics(trackEvent);

			return StatusCode(200);
		}

    private string GetRemoteIp()
    {
      var remoteIp = this.HttpContext.Connection.RemoteIpAddress.ToString();
      if (this.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var proxyIp))
      {
        remoteIp = proxyIp[0];
      }

      return remoteIp;
    }
	}

  /// <summary>
  /// old track request
  /// </summary>
  public class UrlTrackRequest
  {
		public String Url { get; set; }
		public String Topic { get; set; }
		public String Duration { get; set; }
  }

  /// <summary>
  /// track a clicked link
  /// </summary>
  public class TrackLinkClickRequest
  {
		public String Url { get; set; }
  }

  /// <summary>
  /// track request for multiple tracked topics
  /// </summary>
  public class TrackTopicsRequest
  {
		public String Url { get; set; }
		public String Duration { get; set; }
		public TrackTopic[] Topics { get; set; }
  }
}
