namespace ruttmann.vita.api
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Web;

  internal class VitaAuthService : IAuthService
  {
    private readonly IVitaDataService dataService;

    private Dictionary<string, ValidCodeAccess> cookieToCode = new Dictionary<string, ValidCodeAccess>();

    public VitaAuthService(IVitaDataService dataService)
    {
      this.dataService = dataService;
    }

    /// <inheritdoc/>
    public bool IsValidCode(string requestedCode, out string sessionCookie)
    {
      if (this.dataService.IsValidCode(requestedCode))
      {
        var accessToken = new ValidCodeAccess(requestedCode);
        this.cookieToCode[accessToken.Cookie] = accessToken;

        sessionCookie = accessToken.Cookie;
        return true;
      }

      sessionCookie = string.Empty;
      return false;
    }

    /// <summary>
    /// Verify if the cookie is known
    /// </summary>
    /// <param name="cookie">the cookie of the request</param>
    /// <param name="code">the code for the cookie</param>
    /// <returns>true if the cookie was valid and the code is </returns>
    public bool IsValidCookie(string cookie, out string code)
    {
      if (!this.cookieToCode.TryGetValue(cookie, out var accessToken))
      {
        code = String.Empty;
        return false;
      }

      if (DateTime.UtcNow - accessToken.ValidationTime > TimeSpan.FromHours(2))
      {
        this.cookieToCode.Remove(cookie);
        code = String.Empty;
        return false;
      }

      code = accessToken.Code;
      return true;
    }

    /// <summary>
    /// class to store valid access items.
    /// </summary>
    private class ValidCodeAccess
    {
      private const Int32 CookieLength = 32;

      public ValidCodeAccess(string code)
      {
        this.Code = code;
        this.Cookie = GenerateRandomCookie();
        this.ValidationTime = DateTime.UtcNow;
      }

      public string Code { get; }
      public string Cookie { get; }
      public DateTime ValidationTime { get; }

      private String GenerateRandomCookie()
      {
        var cookieBytes = new Byte[CookieLength];
        new Random().NextBytes(cookieBytes);
        var cookie = Convert.ToBase64String(cookieBytes).Replace('+', 'x').Replace('/', 'X').Replace('=', 'w');

        return cookie.Substring(0, CookieLength);
      }
    }
  }
}