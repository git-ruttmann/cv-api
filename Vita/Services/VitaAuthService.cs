using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Vita.Test")]

namespace ruttmann.vita.api
{
  using System;
  using System.Collections.Generic;

  internal class VitaAuthService : IAuthService
  {
    private readonly IVitaDataService dataService;

    private readonly ITimeSource timeSource;

    private Dictionary<string, ValidCodeAccess> cookieToCode = new Dictionary<string, ValidCodeAccess>();

    public VitaAuthService(IVitaDataService dataService, ITimeSource timeSource = null)
    {
      this.dataService = dataService;
      this.timeSource = timeSource ?? new UtcSource();
    }

    /// <inheritdoc/>
    public bool IsValidCode(string requestedCode, out IAuthenticatedSession session)
    {
      if (this.dataService.IsValidCode(requestedCode))
      {
        var customAnimation = this.dataService.GetCustomAnimationForCode(requestedCode);
        var accessToken = new ValidCodeAccess(requestedCode, this.timeSource.Now, customAnimation);
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

      if (this.timeSource.Now - accessToken.ValidationTime > TimeSpan.FromHours(2))
      {
        this.cookieToCode.Remove(cookie);
        session = null;
        return false;
      }

      session = accessToken;
      return true;
    }

    /// <summary>
    /// Provide the current UTC time
    /// </summary>
    private class UtcSource : ITimeSource
    {
      public DateTime Now => DateTime.UtcNow;
    }

    /// <summary>
    /// class to store valid access items.
    /// </summary>
    private class ValidCodeAccess : IAuthenticatedSession
    {
      private const Int32 CookieLength = 430;

      public ValidCodeAccess(string code, DateTime now, string customAnimation)
      {
        this.Code = code;
        this.Cookie = GenerateRandomCookie();
        this.Key = Guid.NewGuid().ToString();
        this.ValidationTime = now;
        this.CustomAnimation = customAnimation;
      }

      public string Code { get; }

      public string Cookie { get; }

      public DateTime ValidationTime { get; }

      public string Key { get; }

      public string CustomAnimation { get; }

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