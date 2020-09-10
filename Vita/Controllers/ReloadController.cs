namespace ruttmann.vita.api.Controllers
{
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Mvc;

  /// <summary>
  /// A simple health check
  /// </summary>
  [Route("api/v1/[controller]")]
  public class ReloadController : Controller
  {
    private readonly IVitaDataService dataService;

    /// <summary>
    /// Create a new reload controller
    /// </summary>
    /// <param name="dataService"></param>
    public ReloadController(IVitaDataService dataService)
    {
      this.dataService = dataService;
    }

    /// <summary>
    /// reload data service
    /// </summary>
    [HttpGet]
    public void Get()
    {
      this.dataService.Reload();
    }
  }
}
