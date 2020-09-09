namespace ruttmann.vita.api
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Microsoft.Extensions.Configuration;

  /// <summary>
  /// service to ask for vita entries
  /// </summary>
  public class VitaDataService : IVitaDataService
  {
    private readonly IFileSystem fileSystem;

    private readonly IConfiguration configuration;

    private readonly string[] configuredFiles;

    private VitaEntry[] database;

    private ISet<string> knownCodes;

    /// <summary>
    /// Create an instance with files from a configuration file
    /// </summary>
    /// <param name="configuration">the configuration</param>
    public VitaDataService(IConfiguration configuration)
    {
      this.fileSystem = new DiskFileSystem();
      this.configuration = configuration;
      this.configuredFiles = new string[0];
    }

    /// <summary>
    /// Create an instance with defined file items
    /// </summary>
    /// <param name="fileSystem">the files system to read the files</param>
    /// <param name="configuredFiles">the configured files</param>
    private VitaDataService(IFileSystem fileSystem, IEnumerable<string> configuredFiles)
    {
      this.fileSystem = fileSystem;
      this.configuration = null;
      this.configuredFiles = configuredFiles.ToArray();
    }

    /// <summary>
    /// Create a mocked data service
    /// </summary>
    /// <param name="fileSystem"></param>
    /// <param name="configuredFiles"></param>
    /// <returns></returns>
    internal static IVitaDataService CreateMockedService(IFileSystem fileSystem, IEnumerable<string> configuredFiles)
    {
      var dataService = new VitaDataService(fileSystem, configuredFiles);
      return dataService;
    }

    /// <inheritdoc/>
    public VitaEntryCollection GetEntriesForCode(String code)
    {
      this.LoadOnDemand();

      var selectedEntries = this.database
        .Where(x => FilterMatchesCode(code, x.Codes))
        .Select(x => new VitaEntryForSerialization(x));
      return new VitaEntryCollection(selectedEntries);
    }

    /// <inheritdoc/>
    public bool IsValidCode(string code)
    {
      this.LoadOnDemand();
      return this.knownCodes.Contains(code);
    }

    private void LoadOnDemand()
    {
      if (this.database != null && Environment.OSVersion.Platform != PlatformID.Win32NT)
      {
        return;
      }

      var fileItems = this.configuredFiles;
      if (this.configuration != null)
      {
        fileItems = this.configuration.GetSection("DataSourceFileSystem")
          .AsEnumerable()
          .Select(x => x.Value)
          .ToArray();
      }

      var itemList = new List<VitaEntry>();
      foreach (var fileItem in fileItems)
      {
        if (fileSystem.TryGetStream(fileItem, out var fileStream))
        {
          var reader = new VitaStreamReader(fileSystem, fileStream, Encoding.UTF8);
          itemList.AddRange(reader.ReadEntries());
        }
      }

      this.database = itemList.ToArray();

      this.knownCodes = itemList.SelectMany(x => x.Codes).Where(x => x != "*").ToHashSet();
    }

    private bool FilterMatchesCode(string code, ISet<string> codes)
    {
      if (codes.Contains(code))
      {
        return true;
      }

      if (codes.Contains("*"))
      {
        return true;
      }

      // Console.WriteLine($"{code} does not match {String.Join(" ", codes)}");
      return false;
    }
  }
}