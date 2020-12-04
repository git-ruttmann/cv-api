namespace ruttmann.vita.api
{
  using System;
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Threading.Tasks;
  using Azure.Core;
  using Azure.Identity;
  using Azure.Security.KeyVault.Secrets;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json.Linq;

  public class LinkedInOAuthService : ILinkedInOAuthService
  {
    private const String LinkedInAuthEndpoint = "https://www.linkedin.com/oauth/v2/accessToken";

    private const String LinkedInMeEndpoint = "https://api.linkedin.com/v2/me";

    private const string KeyVaultEnvironment = "UseAzureKeyVault";

    private readonly string clientId;

    private readonly string clientSecret;

    private readonly IHttpClientFactory httpClientFactory;

    public LinkedInOAuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
      if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(KeyVaultEnvironment)))
      {
        this.clientId = GetKeyVaultSecret("LinkedInClientId");
        this.clientSecret = GetKeyVaultSecret("LinkedInClientSecret");
      }
      else
      {
        this.clientId = configuration["Authentication:LinkedIn:ClientId"];
        this.clientSecret = configuration["Authentication:LinkedIn:ClientSecret"];
      }

      this.httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc/>
    public async Task<string> Authenticate(string code)
    {
      TrackingService.AppendLog($"auth with client id {this.clientId}");
      var formData = this.EncodeOauthData(code);

      var client = this.httpClientFactory.CreateClient("linkedin");
      var response = await client.PostAsync(LinkedInAuthEndpoint, formData);
      var responseData = response.Content.ReadAsStringAsync();

      TrackingService.AppendLog("auth result");
      TrackingService.AppendLog(await responseData);

      if (!response.IsSuccessStatusCode)
      {
        return string.Empty;
      }

      var accessToken = JObject.Parse(await responseData).SelectToken("$.access_token").Value<string>();
      return accessToken;
    }

    /// <inheritdoc/>
    public async Task<string> GetLinkedInUser(string accessToken)
    {
      var me = this.ReadLinkedInMe(accessToken);
      return ExtractNameFromLinkedIn(await me);
    }

    /// <summary>
    /// Get the SAS string from the key vault
    /// </summary>
    /// <param name="key">the name of the key</param>
    /// <returns>the SAS string</returns>
    private static string GetKeyVaultSecret(string key)
    {
      var keyVaultName = Environment.GetEnvironmentVariable(KeyVaultEnvironment);

      SecretClientOptions options = new SecretClientOptions()
      {
          Retry =
          {
              Delay= TimeSpan.FromSeconds(2),
              MaxDelay = TimeSpan.FromSeconds(16),
              MaxRetries = 5,
              Mode = RetryMode.Exponential
          }
      };
      
      var client = new SecretClient(new Uri(keyVaultName), new DefaultAzureCredential(), options);

      var secret = client.GetSecret(key);
      return secret.Value.Value;
    }

    private FormUrlEncodedContent EncodeOauthData(string code)
    {
      var formKvp = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("grant_type", "authorization_code"),
        new KeyValuePair<string, string>("code", code),
        new KeyValuePair<string, string>("redirect_uri", "https://cv.ruttmann.name/oauthsuccess"),
        // new KeyValuePair<string, string>("redirect_uri", "http://localhost:4200/oauthsuccess"),
        new KeyValuePair<string, string>("client_id", this.clientId),
        new KeyValuePair<string, string>("client_secret", this.clientSecret),
      };

      var formData = new FormUrlEncodedContent(formKvp);
      return formData;
    }

    async private Task<string> ReadLinkedInMe(string accessToken)
    {
      try
      {
        var client = this.httpClientFactory.CreateClient("linkedin");
        var request = new HttpRequestMessage(HttpMethod.Get, LinkedInMeEndpoint);
        request.Headers.Add("Authorization", new[] { "Bearer " + accessToken });
        var response = await client.SendAsync(request);

        var contactJsonString = await response.Content.ReadAsStringAsync();
        return contactJsonString;
      }
      catch (Exception)
      {
        return "unknown";
      }
    }

    private string ExtractNameFromLinkedIn(string meAsJson)
    {
      var jsonObject = JObject.Parse(meAsJson);

      var name = jsonObject.SelectToken("$.localizedLastName").Value<string>()
        + " "
        + jsonObject.SelectToken("$.localizedFirstName").Value<string>();

      return name;
    }
  }
}