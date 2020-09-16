namespace ruttmann.vita.api.Controllers
{
	using System;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;

  [Route("api/v1/[controller]")]
  public class AuthenticateController : Controller
  {
    /// <summary>
    /// the header name for the posted login code
    /// </summary>
    const String CodeHeaderName = "VitaLoginCode";

    const String AuthCookieHeaderName = "VitaApiAuth";

    public AuthenticateController(IWebHostEnvironment env)
    {
        this.IsDevelopmentMode = env.IsDevelopment();
    }

    public bool IsDevelopmentMode { get; }

    /// <summary>
    /// post login information
    /// </summary>
    /// <param name="value">the value</param>
    [HttpPost]
    public void Post([FromForm]CodeCheckRequest value)
    {
      var authService = this.HttpContext.RequestServices.GetRequiredService<IAuthService>();

      if (authService.IsValidCode(value.LoginCode, out var session))
      {
        this.Response.StatusCode = 200;

        var allowHacks = this.IsDevelopmentMode && value.LoginCode.StartsWith("x");
        
        var cookieOptions = new CookieOptions() 
        {
          Secure = !allowHacks,
          Expires = DateTime.UtcNow + TimeSpan.FromHours(2),
          SameSite = allowHacks ? SameSiteMode.None : SameSiteMode.Strict,
        };

        this.Response.Cookies.Append(AuthCookieHeaderName, session.Cookie, cookieOptions);
        return;
      }

      this.Response.StatusCode = 401;
    }
  }

  /// <summary>
  /// this 'login' form POST data
  /// </summary>
  public class CodeCheckRequest
  {
    public String LoginCode { get; set; }
  }
}
