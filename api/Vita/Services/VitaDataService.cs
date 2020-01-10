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
    private readonly IConfiguration configuration;

    private VitaEntry[] database;

    private ISet<string> knownCodes;

    public VitaDataService(IConfiguration configuration)
    {
      this.configuration = configuration;
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
      if (this.database != null)
      {
        return;
      }

      var itemList = new List<VitaEntry>();
      var fileItems = this.configuration.GetSection("DataSourceFileSystem").AsEnumerable().Select(x => x.Value).ToArray();
      foreach (var fileItem in fileItems)
      {
        if (File.Exists(fileItem))
        {
          var reader = new VitaStreamReader(new FileStream(fileItem, FileMode.Open, FileAccess.Read), Encoding.UTF8);
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