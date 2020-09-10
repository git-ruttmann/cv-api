namespace Vita.Test
{
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using ruttmann.vita.api;

  /// <summary>
  /// A mocked version of the file system
  /// </summary>
  public class FileSystemMock : IFileSystem
  {
    private readonly Dictionary<string, string> contents;

    /// <summary>
    /// Create a new instance of a file system mock
    /// </summary>
    public FileSystemMock()
    {
        this.contents = new Dictionary<string, string>();
    }

    /// <summary>
    /// Add mocked contents
    /// </summary>
    /// <param name="filename">the simulated file name</param>
    /// <param name="content">the content of the file</param>
    public void AddFile(string filename, string content)
    {
      this.contents[filename] = content;
    }

    /// <inheritdoc/>
    public Stream GetIncludedFile(Stream parentStream, string includeFilename)
    {
      return CreateStream(this.contents[includeFilename]);
    }

    /// <inheritdoc/>
    public bool TryGetStream(string filename, out Stream stream)
    {
      if (this.contents.TryGetValue(filename, out var content))
      {
        stream = CreateStream(content);
        return true;
      }

      stream = null;
      return false;
    }

    /// <summary>
    /// Create a stream for the string content
    /// </summary>
    /// <param name="content">the contet text</param>
    /// <returns>a stream for the content</returns>
    private static Stream CreateStream(string content)
    {
      return new MemoryStream(Encoding.UTF8.GetBytes(content));
    }
  }
}