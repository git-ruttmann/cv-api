namespace ruttmann.vita.api
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public enum VitaEntryType
  {
    Interest,
    
    Person,

    Technology,

    Strength,

    Project,

    Introduction,
  };

  [Flags]
  public enum VitaEntryAttribute
  {
    None = 0,

    German = 1 << 0,

    English = 1 << 1,

    Short = 1 << 2,

    Medium = 1 << 3,

    Long = 1 << 4,

    DurationMask = Short | Medium | Long,
    
    LanguageMask = German | English,
  }

  public class VitaEntry
  {
    public VitaEntry(string title, string[] lines, VitaEntryType vitaEntryType, VitaEntryAttribute attributes, String codes)
    {
      this.Title = title;
      this.Lines = lines;
      this.VitaEntryType = vitaEntryType;
      Attributes = attributes;
      var set = codes.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.Trim())
        .ToHashSet();
      this.Codes = set.Contains("*") ? new HashSet<String>() { "*" } : set;
    }

    public string Title { get; }

    public string[] Lines { get; }

    public VitaEntryType VitaEntryType { get; }

    public VitaEntryAttribute Attributes { get; }

    public ISet<string> Codes { get; }
  }

  public class VitaEntryForSerialization
  {
    public VitaEntryForSerialization(VitaEntry entry)
    {        
      this.VitaEntryType = entry.VitaEntryType.ToString();
      this.Title = entry.Title;
      this.Lines = entry.Lines.SkipWhile(x => String.IsNullOrEmpty(x)).ToArray();
      this.Attributes = Enum.GetValues(typeof(VitaEntryAttribute))
        .Cast<VitaEntryAttribute>()
        .Where(x => entry.Attributes.HasFlag(x) && x != VitaEntryAttribute.None)
        .Where(x => x != VitaEntryAttribute.DurationMask && x != VitaEntryAttribute.LanguageMask)
        .Select(x => x.ToString())
        .ToArray();
    }

    public String VitaEntryType { get; set; }

    public string Title { get; set; }

    public string[] Lines { get; set; }

    public string[] Attributes { get; set; }
  }

  public class VitaEntryCollection
  {
    public VitaEntryCollection(IEnumerable<VitaEntryForSerialization> vitaEntries)
    {
        this.Entries = vitaEntries.ToArray();
    }

    public VitaEntryForSerialization[] Entries { get; }
  }
}
