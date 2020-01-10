namespace ruttmann.vita.api.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.DependencyInjection;

  [Route("api/v1/[controller]")]
	public class VitaController : Controller
	{
		/// <summary>
		/// Get the collection of vita entries for the active code.
		/// </summary>
		/// <returns>a vita collection</returns>
		[HttpGet]
		[Produces("application/json")]
		[Authorize]
		public VitaEntryCollection Get()
		{
			var vitaDataService = this.HttpContext.RequestServices.GetRequiredService<IVitaDataService>();
			var activeCode = this.HttpContext.Request.Headers["Code"].Single();
			return vitaDataService.GetEntriesForCode(activeCode);
		}
	}
}
