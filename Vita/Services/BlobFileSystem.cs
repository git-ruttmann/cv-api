namespace ruttmann.vita.api
{
  using System;
  using System.IO;
  using Azure.Core;
  using Azure.Identity;
  using Azure.Security.KeyVault.Secrets;
  using Microsoft.Azure.Storage.Blob;
  using Microsoft.Extensions.Configuration;

  /// <summary>
  /// File system for disk files
  /// </summary>
  public class BlobFileSystem : IFileSystem
  {
    private readonly CloudBlobContainer blobContainer;

    public BlobFileSystem(IConfiguration configuration)
    {
      var connection = GetStorageAccountSas();
      var blobClient = new CloudBlobClient(new Uri(connection));
      this.blobContainer = blobClient.GetContainerReference("cvdata");
    }

    /// <inheritdoc/>
    public Stream GetIncludedFile(Stream parentStream, string includeFilename)
    {
      return this.CreateStream(includeFilename);
    }

    /// <inheritdoc/>
    public bool TryGetStream(string filename, out Stream stream)
    {
      if (filename == null || !filename.StartsWith("/"))
      {
        stream = null;
        return false;
      }

      var blobItem = Path.GetFileName(filename); 

      if (!this.blobContainer.GetBlockBlobReference(blobItem).Exists())
      {
        stream = null;
        return false;
      }

      stream = this.CreateStream(blobItem);
      return true;
    }

    /// <summary>
    /// Create a stream for the blob item
    /// </summary>
    /// <param name="blobItem">the item inside the blob</param>
    /// <returns>the stream</returns>
    private Stream CreateStream(string blobItem)
    {
      var stream = new MemoryStream();
      var blob = this.blobContainer.GetBlockBlobReference(blobItem);
      blob.DownloadToStream(stream);
      stream.Seek(0, SeekOrigin.Begin);
      return stream;
    }

    /// <summary>
    /// Get the SAS string from the key vault
    /// </summary>
    /// <returns>the SAS string</returns>
    private static string GetStorageAccountSas()
    {
      var keyVaultName = Environment.GetEnvironmentVariable("UseAzureKeyVault");

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

      var secret = client.GetSecret("cvdata-sas");
      return secret.Value.Value;
    }
  }
}
