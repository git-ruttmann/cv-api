namespace ruttmann.vita.api
{
  using System;
  using System.Collections.Generic;
  using System.IO;
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

    private Dictionary<string, string[]> codes;

    /// <summary>
    /// Create an instance with files from a configuration file
    /// </summary>
    /// <param name="configuration">the configuration</param>
    public VitaDataService(IConfiguration configuration, IFileSystem fileSystem)
    {
      this.fileSystem = fileSystem;
      this.configuration = configuration;
      this.configuredFiles = new string[0];
      this.codes = new Dictionary<string, string[]>();
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

      var groups = this.codes[code].ToHashSet();

      var selectedEntries = this.database
        .Where(x => FilterMatchesCode(code, groups, x.Codes))
        .Select(x => new VitaEntryForSerialization(x));
      return new VitaEntryCollection(selectedEntries);
    }

    /// <inheritdoc/>
    public bool IsValidCode(string code)
    {
      this.LoadOnDemand();
      if (code == null)
      {
        return false;
      }

      return this.codes.ContainsKey(code);
    }

    /// <inheritdoc/>
    public void Reload()
    {
      this.database = null;
      this.LoadOnDemand();
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
          if (Path.GetFileName(fileItem) == "codes")
          {
            var reader = new CodesStreamReader(fileStream, Encoding.UTF8);
            this.codes = reader.ReadCodes().ToDictionary(x => x.Key, x => x.Value);
          }
          else
          {
            var reader = new VitaStreamReader(fileSystem, fileStream, Encoding.UTF8);
            itemList.AddRange(reader.ReadEntries());
          }
        }
      }

      this.database = itemList.ToArray();
    }

    private bool FilterMatchesCode(string code, ISet<string> groups, ISet<string> topicCodes)
    {
      if (topicCodes.Contains(code))
      {
        return true;
      }

      if (topicCodes.Contains("-" + code))
      {
        return false;
      }

      if (topicCodes.Intersect(groups).Any())
      {
        return true;
      }

      // Console.WriteLine($"{code} does not match {String.Join(" ", codes)}");
      return false;
    }
  }
}
