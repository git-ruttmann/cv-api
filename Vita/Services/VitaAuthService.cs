using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Vita.Test")]

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
    public bool IsValidCode(string requestedCode, out IAuthenticatedSession session)
    {
      if (this.dataService.IsValidCode(requestedCode))
      {
        var accessToken = new ValidCodeAccess(requestedCode);
        this.cookieToCode[accessToken.Cookie] = accessToken;

        session = accessToken;
        return true;
      }

      session = null;
      return false;
    }

    /// <summary>
    /// Verify if the cookie is known
    /// </summary>
    /// <param name="cookie">the cookie of the request</param>
    /// <param name="code">the code for the cookie</param>
    /// <returns>true if the cookie was valid and the code is </returns>
    public bool IsValidCookie(string cookie, out IAuthenticatedSession session)
    {
      if (!this.cookieToCode.TryGetValue(cookie, out var accessToken))
      {
        session = null;
        return false;
      }

      if (DateTime.UtcNow - accessToken.ValidationTime > TimeSpan.FromHours(2))
      {
        this.cookieToCode.Remove(cookie);
        session = null;
        return false;
      }

      session = accessToken;
      return true;
    }

    /// <summary>
    /// class to store valid access items.
    /// </summary>
    private class ValidCodeAccess : IAuthenticatedSession
    {
      private const Int32 CookieLength = 32;

      public ValidCodeAccess(string code)
      {
        this.Code = code;
        this.Cookie = GenerateRandomCookie();
        this.Key = Guid.NewGuid().ToString();
        this.ValidationTime = DateTime.UtcNow;
      }

      public string Code { get; }

      public string Cookie { get; }

      public DateTime ValidationTime { get; }

      public string Key { get; }

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