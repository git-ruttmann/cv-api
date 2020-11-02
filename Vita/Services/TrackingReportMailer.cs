namespace ruttmann.vita.api
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net.Mail;

  public interface ITrackingReportMailer
  {
  }
  
  public class TrackingReportMailer : ITrackingReportMailer
  {
    public TrackingReportMailer(ITrackingService trackingService)
    {
      trackingService.Reports.Subscribe(x => this.HandleNewReport(x));
    }

    internal static String FormatDuration(TimeSpan timeSpan)
    {
      return TimeSpan.FromSeconds((Int64) (timeSpan.TotalSeconds + 0.8)).ToString("c");
    }

    private void HandleNewReport(ITrackingReport report)
    {
      if (report.Code.StartsWith("x"))
      {
        TrackingService.AppendLog($"do not send report to {report.Code}");
        return;
      }

      TrackingService.AppendLog("generating mail");

      var messageLines = new List<String>();
      messageLines.Add($"Login code '{report.Code}' from: {report.Ip} at {report.StartTime} for {FormatDuration(report.Duration)}");
      messageLines.Add(String.Empty);
      messageLines.AddRange(report.Topics.Select(x => $"{x.Url}/{x.TopicDetail}/{x.Title} for { FormatDuration(x.ImpressionTimeSpan)}"));
      messageLines.Add(String.Empty);
      messageLines.Add("Links:");
      messageLines.AddRange(report.ClickedLinks);

      var mail = new MailMessage("cv@ruttmann.name", "matthias@ruttmann.name");
      mail.Subject = $"CV report for {report.Code}, duration {report.Duration}";

      mail.Body = String.Join("\r\n", messageLines);
      mail.IsBodyHtml = false;
      var client = new SmtpClient("mrxn.de", 587);
      client.Send(mail);

      TrackingService.AppendLog("mail sent");
    }
  }
}