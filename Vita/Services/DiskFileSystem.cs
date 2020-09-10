namespace ruttmann.vita.api
{
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using Microsoft.Extensions.Configuration;

  /// <summary>
  /// File system for disk files
  /// </summary>
  public class DiskFileSystem : IFileSystem
  {
    /// <inheritdoc/>
    public Stream GetIncludedFile(Stream parentStream, string includeFilename)
    {
      var streamFilename = (parentStream as FileStream)?.Name;
      var filename = Path.Join(Path.GetDirectoryName(streamFilename), includeFilename);
      return CreateStream(filename);
    }

    /// <inheritdoc/>
    public bool TryGetStream(string filename, out Stream stream)
    {
      if (!File.Exists(filename))
      {
        stream = null;
        return false;
      }

      stream = CreateStream(filename);
      return true;
    }

    /// <summary>
    /// Create a stream for the file
    /// </summary>
    /// <param name="fullpath">the full path name</param>
    /// <returns>the stream</returns>
    private Stream CreateStream(string fullpath)
    {
      return new FileStream(fullpath, FileMode.Open, FileAccess.Read);
    }
  }
}