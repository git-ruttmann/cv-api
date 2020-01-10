namespace ruttmann.vita.api
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text;

  public class VitaStreamReader
  {
    private readonly Stream stream;

    private readonly Encoding encoding;

    private bool inHeaderSection;

    private VitaEntryBuilder builder = null;

    private String globalCode = String.Empty;

    public VitaStreamReader(Stream stream, Encoding encoding)
    {
      this.encoding = encoding;
      this.stream = stream;
      this.globalCode = "*";
      this.inHeaderSection = false;
    }

    public IEnumerable<VitaEntry> ReadEntries()
    {
      using (var reader = new StreamReader(this.stream, this.encoding))
      {
        String line;

        while ((line = reader.ReadLine()) != null)
        {
          if (line.StartsWith("##"))
          {
            if (this.CloseSectionAndValidate())
            {
              yield return this.builder.BuildEntry();
            }

            this.BeginSection(line);
          }
          else if (builder == null)
          {
            if (String.IsNullOrEmpty(line))
            {
              continue;
            }
            else if (line.StartsWith("#code:"))
            {
              this.globalCode = line.Split(':', 2)[1].Trim();
            }
            else
            {
              throw new InvalidDataException("file must start with ##<vita entry type>");
            }
          }
          else if (this.inHeaderSection && line.StartsWith("#"))
          {
            this.HandleHeaderAttribute(line);
          }
          else
          {
            this.inHeaderSection = false;
            this.builder.Lines.Add(line);
          }
        }

        if (this.CloseSectionAndValidate())
        {
          yield return this.builder.BuildEntry();
        }
      }
    }

    private void HandleHeaderAttribute(string line)
    {
      if (line.StartsWith("#code:"))
      {
        builder.SetCode(line.Split(':', 2)[1].Trim());
      }
      else if (line.StartsWith("#attributes:"))
      {
        builder.SetAttributes(line.Split(':', 2)[1].Trim());
      }
      else
      {
        throw new InvalidDataException("bad code: {line}");
      }
    }

    private void BeginSection(string line)
    {
      var tokens = line.Substring(2).Split(':', 2);
      object vitaEntryType;
      if (!Enum.TryParse(typeof(VitaEntryType), tokens[0], true, out vitaEntryType))
      {
        throw new InvalidDataException($"unkown vita type: {tokens[0]}");
      }

      if (tokens.Length != 2)
      {
        throw new InvalidDataException($"missing title in line: {line}");
      }

      this.builder = new VitaEntryBuilder((VitaEntryType)vitaEntryType, tokens[1].Trim());
      this.inHeaderSection = true;
    }

    private bool CloseSectionAndValidate()
    {
      if (builder != null)
      {
        if (String.IsNullOrEmpty(builder.Code))
        {
          builder.SetCode(this.globalCode);
        }

        return true;
      }

      return false;
    }

    private class VitaEntryBuilder
    {
      public VitaEntryBuilder(VitaEntryType vitaEntryType, String title)
      {
        this.Lines = new List<string>();
        this.Title = title;
        this.VitaEntryType = vitaEntryType;
        this.Attributes = VitaEntryAttribute.None;
        this.Code = String.Empty;
      }

      public string Title { get; }

      public VitaEntryType VitaEntryType { get; }

      public List<String> Lines { get; }

      public VitaEntryAttribute Attributes { get; private set; }

      public string Code { get; private set; }

      public VitaEntry BuildEntry()
      {
        if ((this.Attributes & VitaEntryAttribute.LanguageMask) == VitaEntryAttribute.None)
        {
          this.Attributes |= VitaEntryAttribute.LanguageMask;
        }

        if ((this.Attributes & VitaEntryAttribute.DurationMask) == VitaEntryAttribute.None)
        {
          this.Attributes |= VitaEntryAttribute.DurationMask;
        }

        if (String.IsNullOrEmpty(this.Code))
        {
          this.Code = "*";
        }

        return new VitaEntry(this.Title, this.Lines.ToArray(), this.VitaEntryType, this.Attributes, this.Code);
      }

      public void SetAttributes(String attributes)
      {
        this.Attributes |= attributes
          .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
          .Select(x => x.Trim())
          .Select(x =>
          {
            object vitaEntryAttribute;
            if (!Enum.TryParse(typeof(VitaEntryAttribute), x, true, out vitaEntryAttribute))
            {
              throw new InvalidDataException($"unknown attribute: '{x}'");
            }

            return (VitaEntryAttribute)vitaEntryAttribute;
          })
          .Aggregate(VitaEntryAttribute.None, (x, y) => x | y);
      }

      public void SetCode(string code)
      {
        if (String.IsNullOrEmpty(this.Code)) 
        {
          this.Code = code;
        }
        else 
        {
          this.Code = this.Code + " " + code;
        }
      }
    }
  }
}
