namespace ruttmann.vita.api
{
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text;

  internal class CodesStreamReader
  {
    private readonly Stream fileStream;
    private readonly Encoding encoding;

    /// <summary>
    /// Create a new stream reader to read codes
    /// </summary>
    /// <param name="fileStream">the input stream</param>
    /// <param name="encoding">the encoding</param>
    public CodesStreamReader(Stream fileStream, Encoding encoding)
    {
      this.fileStream = fileStream;
      this.encoding = encoding;
    }

    /// <summary>
    /// Read the codes from the stream
    /// </summary>
    /// <returns>tuples of code and groups for the code</returns>
    public IEnumerable<KeyValuePair<string, string[]>> ReadCodes()
    {
      using(var reader = new StreamReader(this.fileStream, this.encoding))
      {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
          var code = line.Split(':');
          if (code.Length == 2)
          {
              var groups = code[1]
                .Split(' ')
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .ToArray();
              yield return new KeyValuePair<string, string[]>(code[0], groups);
          }
        }
      }
    }
  }
}