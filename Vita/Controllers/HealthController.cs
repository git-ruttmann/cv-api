namespace ruttmann.vita.api.Controllers
{
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Mvc;

  /// <summary>
  /// A simple health check
  /// </summary>
  [Route("api/v1/[controller]")]
  public class HealthController : Controller
  {
    public HealthController(IHostingEnvironment env)
    {
    }

    /// <summary>
    /// post login information
    /// </summary>
    /// <param name="value">the value</param>
    [HttpGet]
    public void Get()
    {
      this.Response.StatusCode = 200;
    }
  }
}
