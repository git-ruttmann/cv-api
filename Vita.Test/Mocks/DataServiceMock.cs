namespace Vita.Test
{
  using System.Linq;
  using ruttmann.vita.api;

  public class DataServiceMock : IVitaDataService
  {
    private readonly VitaEntry[] vitaEntries;

    /// <summary>
    /// Create a mocked data service with some fake entries
    /// </summary>
    public DataServiceMock()
    {
      this.vitaEntries = new VitaEntry[]
      {
        new VitaEntry("Person 1", new[] {"Lorem ipsum dolor"}, VitaEntryType.Person, VitaEntryAttribute.English | VitaEntryAttribute.Short, "test, t2"),
        new VitaEntry("Person 2", new[] {"At vero eos et accusam"}, VitaEntryType.Person, VitaEntryAttribute.English | VitaEntryAttribute.Short, "test, t2"),
        new VitaEntry("Person 3", new[] {"Stet clita kasd gubergren"}, VitaEntryType.Person, VitaEntryAttribute.English | VitaEntryAttribute.Short, "t2"),
        new VitaEntry("Person 4", new[] {"Duis autem vel eum iriure dolo"}, VitaEntryType.Person, VitaEntryAttribute.English | VitaEntryAttribute.Short, "test"),
      };
    }

    /// <inheritdoc/>
    public string GetCustomAnimationForCode(string code)
    {
      return string.Empty;
    }

    /// <inheritdoc/>
    public VitaEntryCollection GetEntriesForCode(string code)
    {
      var selectedEntries = this.vitaEntries
        .Where(x => x.Codes.Contains(code))
        .Select(x => new VitaEntryForSerialization(x));

      return new VitaEntryCollection(selectedEntries);
    }

    /// <inheritdoc/>
    public bool IsValidCode(string code)
    {
      if (code == "test")
      {
        return true;
      }
      else if (code == "t2")
      {
        return true;
      }

      return false;
    }

    /// <inheritdoc/>
    public void Reload()
    {      
    }
  }
}