namespace ruttmann.vita.api
{
  using System.Threading.Tasks;

  /// <summary>
  /// linkedin OAuth2 and UserInfo provider
  /// </summary>
  public interface ILinkedInOAuthService
  {
    /// <summary>
    /// authenticate the code with the OAuth server
    /// </summary>
    /// <param name="code">the code</param>
    /// <returns>a validated access token</returns>
    Task<string> Authenticate(string code);

    /// <summary>
    /// Get the user name in linked in
    /// </summary>
    /// <param name="accessToken">the access token</param>
    /// <returns>the user name</returns>
    Task<string> GetLinkedInUser(string accessToken);
  }
}