namespace ruttmann.vita.api
{
  using System;

  public interface IVitaDataService
  {
    /// <summary>
    /// get all vita entries for a code
    /// </summary>
    /// <param name="code">the code</param>
    /// <returns>an object with a collection of vita entries</returns>
    VitaEntryCollection GetEntriesForCode(String code);

    /// <summary>
    /// Check if the code is known in the database
    /// </summary>
    /// <param name="code">the code to test</param>
    /// <returns>true if the code is valid</returns>
    bool IsValidCode(String code);

    /// <summary>
    /// reload the data
    /// </summary>
    void Reload();

    /// <summary>
    /// Get the custom animation for a code
    /// </summary>
    /// <param name="code">the code</param>
    /// <returns>the name of a custom animation or an empty string</returns>
    string GetCustomAnimationForCode(string code);
  }
}