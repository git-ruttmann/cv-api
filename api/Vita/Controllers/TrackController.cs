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
    public IActionResult Post([FromBody]TrackRequest value)
		{
			var trackingService = this.HttpContext.RequestServices.GetRequiredService<ITrackingService>();
			var trackEvent = new TrackingEvent(
				this.HttpContext.Request.Headers["Code"].Single(),
				this.HttpContext.Connection.RemoteIpAddress.ToString(),
				value.Url,
				value.Topic,
				value.Scroll);
			trackingService.RecordEvent(trackEvent);
			return StatusCode(200);
		}
	}

  /// <summary>
  /// this 'login' form POST data
  /// </summary>
  public class TrackRequest
  {
		public String Url { get; set; }
		public String Topic { get; set; }
		public Double Scroll { get; set; }
  }
}
