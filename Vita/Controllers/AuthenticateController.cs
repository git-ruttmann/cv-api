namespace ruttmann.vita.api.Controllers
{
	using System;
  using System.Threading.Tasks;

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

    private readonly ILinkedInOAuthService linkedInOAuthService;

    public AuthenticateController(IWebHostEnvironment env, ILinkedInOAuthService linkedInOAuthService)
    {
      this.IsDevelopmentMode = env.IsDevelopment();
      this.linkedInOAuthService = linkedInOAuthService;
    }

    public bool IsDevelopmentMode { get; }

    /// <summary>
    /// post login information
    /// </summary>
    /// <param name="value">the value</param>
    [HttpPost]
		[Produces("application/json")]
    public CodeCheckReply Post([FromForm]CodeCheckRequest value)
    {
      var authService = this.HttpContext.RequestServices.GetRequiredService<IAuthService>();

      if (!authService.IsValidCode(value.LoginCode, value.LoginCode, out var session))
      {
        this.Response.StatusCode = 401;
        return new CodeCheckReply();
      }

      var allowHacks = this.IsDevelopmentMode && value.LoginCode.StartsWith("x");
      
      this.BuildAuthSuccessResponse(allowHacks, session);
      this.Response.StatusCode = 200;
      return new CodeCheckReply {
          CustomAnimation = session.CustomAnimation
        };
    }

    /// <summary>
    /// post login information
    /// </summary>
    /// <param name="value">the value</param>
    [HttpPost("oauth")]
		[Produces("application/json")]
    async public Task<CodeCheckReply> PostOauth([FromBody]OauthLoginRequest value)
    {
      var accessToken = this.linkedInOAuthService.Authenticate(value.OAuthCode);
      if (string.IsNullOrEmpty(await accessToken))
      {
        this.Response.StatusCode = 401;
        return new CodeCheckReply();
      }

      var name = await this.linkedInOAuthService.GetLinkedInUser(accessToken.Result);
      TrackingService.AppendLog("LinkedIn login for " + name + " from " + this.GetRemoteIp());

      var authService = this.HttpContext.RequestServices.GetRequiredService<IAuthService>();
      var loginCode = "linkedin";
      if (!authService.IsValidCode(loginCode, name, out var session))
      {
        this.Response.StatusCode = 401;
        return new CodeCheckReply();
      }

      var allowHacks = this.IsDevelopmentMode && loginCode.StartsWith("x") || true;
      this.BuildAuthSuccessResponse(allowHacks, session);
      return new CodeCheckReply
      {
        CustomAnimation = session.CustomAnimation
      };
    }

    private void BuildAuthSuccessResponse(bool allowHacks, IAuthenticatedSession session)
    {
      var cookieOptions = new CookieOptions()
      {
        Secure = !allowHacks,
        Expires = DateTime.UtcNow + TimeSpan.FromHours(2),
        SameSite = SameSiteMode.Strict,
      };

      this.Response.Cookies.Append(AuthCookieHeaderName, session.Cookie, cookieOptions);

      this.Response.StatusCode = 200;
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
  /// this 'login' form POST data
  /// </summary>
  public class CodeCheckRequest
  {
    public String LoginCode { get; set; }
  }

  /// <summary>
  /// this 'oauth' JSON data
  /// </summary>
  public class OauthLoginRequest
  {
    public String OAuthCode { get; set; }
  }

  public class CodeCheckReply
  {
    public String CustomAnimation { get; set; }

    public String FirstName { get; set; }

    public String Name { get; set; }
  }
}
