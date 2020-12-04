namespace ruttmann.vita.api
{
  using System;

  /// <summary>
  /// Information about an authenticated session
  /// </summary>
  public interface IAuthenticatedSession
  {
    String Code { get; }
    String Name { get; }
    String Key { get; }
    String Cookie { get; }
    String CustomAnimation { get; }
  }

  public interface IAuthService
  {
    /// <summary>
    /// Verify if the cookie is valid and get the code for that cookie
    /// </summary>
    /// <param name="cookie">the cookie</param>
    /// <param name="session">the session information</param>
    /// <returns>true if the cookie is valid</returns>
    Boolean IsValidCookie(string cookie, out IAuthenticatedSession session);

    /// <summary>
    /// Verifies if the code is valid.
    /// </summary>
    /// <param name="requestedCode">the code</param>
    /// <param name="userName">the name of the user</param>
    /// <param name="session">the session information if the code is valid</param>
    /// <returns>true if the code is valid</returns>
    Boolean IsValidCode(string requestedCode, string userName, out IAuthenticatedSession session);
  }
}
