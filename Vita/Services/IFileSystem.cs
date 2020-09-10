namespace ruttmann.vita.api
{
  using System.Collections.Generic;
  using System.IO;

  /// <summary>
  /// provide file streams
  /// </summary>
  public interface IFileSystem
  {
    /// <summary>
    /// get the stream of a file if it exists
    /// </summary>
    /// <param name="filename">the file name</param>
    /// <param name="stream">the stream to the file</param>
    /// <returns>true if the file is found and the stream is open</returns>
    bool TryGetStream(string filename, out Stream stream);

    /// <summary>
    /// get the included file
    /// </summary>
    /// <param name="parentStream">the stream of the file that includes the new file</param>
    /// <param name="includeFilename">name of the included file</param>
    /// <returns>the stream to the file</returns>
    Stream GetIncludedFile(Stream parentStream, string includeFilename);
  }
}