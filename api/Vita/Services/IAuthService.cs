namespace ruttmann.vita.api
{
  using System;
  using System.Collections.Generic;

  public interface IAuthService
  {
    /// <summary>
    /// Verify if the cookie is valid and get the code for that cookie
    /// </summary>
    /// <param name="cookie">the cookie</param>
    /// <param name="code">the code of the cookie</param>
    /// <returns>true if the cookie is valid</returns>
    Boolean IsValidCookie(string cookie, out string code);

    /// <summary>
    /// Verifies if the code is valid.
    /// </summary>
    /// <param name="requestedCode">the code</param>
    /// <param name="sessionCookie">the session cookie if the code is valid</param>
    /// <returns>true if the code is valid</returns>
    bool IsValidCode(string requestedCode, out string sessionCookie);
  }
}
