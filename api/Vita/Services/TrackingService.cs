namespace ruttmann.vita.api
{
  using System;
  using System.IO;
  using System.Text;

  public interface ITrackingService
  {
    void RecordEvent(TrackingEvent trackingEvent);
  }

  public class TrackingEvent
  {
    public TrackingEvent(String code, String ip, String url, String topic, double scroll)
    {
      Code = code;
      Ip = ip;
      Url = url;
      Topic = topic;
      Scroll = scroll;
    }

    public string Code { get; }
    public string Ip { get; }
    public string Url { get; }
    public string Topic { get; }
    public double Scroll { get; }
  }

  public class TrackingService : ITrackingService
  {
    public void RecordEvent(TrackingEvent trackingEvent)
    {
      String filePath;
      if (Environment.OSVersion.Platform != PlatformID.Win32NT)
      {
        filePath = "/var/log/vita.track";
      }
      else
      {
        filePath = @"C:\Users\Ruttmann\Documents\track.txt";
      }
      
      var lineText = DateTime.Now.ToString() + $" ({trackingEvent.Code}/{trackingEvent.Ip}): {trackingEvent.Url} {trackingEvent.Topic} {trackingEvent.Scroll}";

      try
      {
        using (StreamWriter w = File.AppendText(filePath))
        {
          w.WriteLine(lineText);
        }
      }
      catch (IOException)
      {
      }
    }
  }
}