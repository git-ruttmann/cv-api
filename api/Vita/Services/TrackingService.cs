namespace ruttmann.vita.api
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Timers;
  using System.Reactive.Subjects;

  /// <summary>
  /// information about a visible topic
  /// </summary>
  public class TrackTopicsEvent
  {
    public TrackTopicsEvent(String url, String detail, IEnumerable<TrackTopic> topics)
    {
      this.Url = url;
      this.Detail = detail;
      this.Topics = topics.ToArray();
    }

    public string Url { get; }
    public string Detail { get; }
		public TrackTopic[] Topics { get; }
  }

  /// <summary>
  /// information about a visible topic
  /// </summary>
  public class TrackTopic
  {
		public String Topic { get; set; }
		public double Start { get; set; }
		public double End { get; set; }
  }

  public class UrlTrackingEvent
  {
    public UrlTrackingEvent(String url, String detail)
    {
      Url = url;
      Detail = detail;
    }

    public string Url { get; }
    public string Detail { get; }
  }

  public class TrackingService : ITrackingService
  {
    private readonly Dictionary<String, TrackingSession> sessionDict = new Dictionary<string, TrackingSession>();

    private readonly ITimeSource timeSource;

    private Timer reportSendTimer;

    private Subject<ITrackingReport> reportsSubject;

    public TrackingService(ITimeSource timeSource = null)
    {
      this.timeSource = timeSource ?? new RealTimeSource();
      this.reportSendTimer = new Timer(30000);
      this.reportSendTimer.Elapsed += (sender, e) => this.PublishReports(TimeSpan.FromMinutes(5));
      this.reportSendTimer.Start();
      this.reportsSubject =  new Subject<ITrackingReport>();
      this.Reports = this.reportsSubject;
    }

    public IObservable<ITrackingReport> Reports { get; }

    public static void AppendLog(string lineText)
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

      if (!File.Exists(filePath))
      {
        return;
      }
      
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

    public ITrackingSession GetSession(string code, string sessionId, string ip)
    {
      lock(this.sessionDict)
      {
        if (!this.sessionDict.TryGetValue(sessionId, out var session))
        {
          session = new TrackingSession(code, sessionId, ip, this.timeSource);
          sessionDict.Add(sessionId, session);
        }

        return session;
      }
    }

    public void PublishReports()
    {
      this.PublishReports(TimeSpan.Zero);
    }

    private void PublishReports(TimeSpan maxStallTime)
    {
      TrackingSession[] sessions;
      lock(this.sessionDict)
      {
        sessions = this.sessionDict.Values.Where(x => x.IsDirty).ToArray();
      }

      foreach (var session in sessions)
      {
        if (session.LastEventTime + maxStallTime <= this.timeSource.Now)
        {
          session.IsDirty = false;
          this.reportsSubject.OnNext(session.GenerateReport());
        }
      }
    }

    private class RealTimeSource : ITimeSource
    {
      public DateTime Now => DateTime.Now;
    }

    private class TrackingSession : ITrackingSession
    {
      private string sessionId;
      private string ip;
      private readonly ITimeSource timeSource;
      private List<Tuple<DateTime, UrlTrackingEvent>> urlTrackEvents;
      private List<Tuple<DateTime, TrackTopicsEvent>> topicEvents;
      private DateTime createTime;

      public TrackingSession(string code, string sessionId, string ip, ITimeSource timeSource)
      {
        this.Code = code;
        this.sessionId = sessionId;
        this.ip = ip;
        this.timeSource = timeSource;
        this.createTime = timeSource.Now;
        this.topicEvents = new List<Tuple<DateTime, TrackTopicsEvent>>();
        this.urlTrackEvents = new List<Tuple<DateTime, UrlTrackingEvent>>();
      }

      public string Code { get; }

      public Boolean IsDirty { get; set; }

      public DateTime LastEventTime 
      {
        get
        {
          return this.topicEvents.LastOrDefault()?.Item1 ?? this.createTime;
        }
      }

      public void RecordUrl(UrlTrackingEvent urlTrackingEvent)
      {
        this.urlTrackEvents.Add(Tuple.Create(this.timeSource.Now, urlTrackingEvent));
        this.IsDirty = true;

        var lineText = DateTime.Now.ToString() + $" ({this.Code}/{this.ip}): {urlTrackingEvent.Url}/{urlTrackingEvent.Detail}";
        TrackingService.AppendLog(lineText);
      }

      public void RecordTopics(TrackTopicsEvent e)
      {
        this.topicEvents.Add(Tuple.Create(this.timeSource.Now, e));
        this.IsDirty = true;

        var topics = String.Join(",", e.Topics.Select(x => x.Topic + FormatVisibility(x)));
        var lineText = DateTime.Now.ToString() + $" ({this.Code}/{this.ip}): {e.Url}/{e.Detail} {topics}";

        TrackingService.AppendLog(lineText);
      }

      private String FormatVisibility(TrackTopic topic)
      {
        var start = Math.Round(topic.Start * 100, 0);
        var end = Math.Round(topic.End * 100, 0);

        if (start == 0 && end == 100)
        {
          return String.Empty;
        }
        else if (start == 0)
        {
          return $" (..{end})";
        }
        else if (end == 100)
        {
          return $" ({start}..)";
        }
        else
        {
          return $" ({start}-{end})";
        }
      }

      /// <inheritdoc/>
      public ITrackingReport GenerateReport()
      {
        var builder = new TrackingReportBuilder(this.topicEvents, this.urlTrackEvents, this.timeSource.Now);
        return builder.Build(this.Code, this.ip);
      }
    }

    private class TrackingReportBuilder
    {
      private static TimeSpan maxImpressionDuration = TimeSpan.FromSeconds(60);
      private Tuple<DateTime, TrackTopicsEvent>[] topicEvents;
      private Tuple<DateTime, UrlTrackingEvent>[] urlTrackEvents;
      private Dictionary<String, TopicReport> topicDictionary;
      private readonly DateTime now;

      public TrackingReportBuilder(
        IEnumerable<Tuple<DateTime, TrackTopicsEvent>> topicEvents, 
        IEnumerable<Tuple<DateTime, UrlTrackingEvent>> urlTrackEvents,
        DateTime now)
      {
        this.topicEvents = topicEvents.ToArray();
        this.urlTrackEvents = urlTrackEvents.ToArray();
        this.now = now;
      }

      public ITrackingReport Build(string code, string ip)
      {
        if (this.topicEvents.Length == 0)
        {
          return new TrackingReport(code, ip, now, TimeSpan.FromSeconds(0), new ITopicReport[0]);
        }

        var startTime = this.topicEvents.First().Item1;
        var endOfImpressions = startTime;
        topicDictionary = new Dictionary<String, TopicReport>();

        var generation = 0;
        foreach(var topicTuple in this.GetTopicAndDuration())
        {
          var duration = topicTuple.Item1;
          endOfImpressions = endOfImpressions + duration;

          var topicCollection = topicTuple.Item2;
          foreach (var topicEvent in topicCollection.Topics)
          {
            var topicReport = this.GetTopicReport(topicCollection, topicEvent);
            if (topicReport.LastVisibleGeneration != generation - 1)
            {
              topicReport.ImpressionCount += 1;
            }

            topicReport.LastVisibleGeneration = generation;
            topicReport.ImpressionTimeSpan += duration * (topicEvent.End - topicEvent.Start);
          }

          generation++;
        }

        return new TrackingReport(code, ip, startTime, endOfImpressions - startTime, topicDictionary.Values);
      }

      private TopicReport GetTopicReport(TrackTopicsEvent topicCollection, TrackTopic topicEvent)
      {
        var key = topicEvent.Topic + "#" + topicCollection.Detail + "#" + topicCollection.Url;
        if (!topicDictionary.TryGetValue(key, out var topicReport))
        {
          topicReport = new TopicReport(topicEvent.Topic, topicCollection.Detail, topicCollection.Url);
          topicReport.LastVisibleGeneration = Int32.MinValue;
          topicDictionary[key] = topicReport;
        }

        return topicReport;
      }

      private IEnumerable<Tuple<TimeSpan, TrackTopicsEvent>> GetTopicAndDuration()
      {
        var urlTrackEvents = this.urlTrackEvents
          .Where(x => x.Item2.Url == "/")
          .Concat(new[] { Tuple.Create(this.now + TimeSpan.FromDays(1), new UrlTrackingEvent("/", String.Empty)) })
          .ToArray();

        var nextUrlEvent = 0;
        for (int i = 0; i < this.topicEvents.Length; i++)
        {
          var nextEventTime = i + 1 < this.topicEvents.Length ? this.topicEvents[i + 1].Item1 : DateTime.Now;
          var currentEventTime = this.topicEvents[i].Item1;

          while (nextUrlEvent < urlTrackEvents.Length - 1 && currentEventTime >= urlTrackEvents[nextUrlEvent].Item1)
          {
            nextUrlEvent++;
          }

          if (nextEventTime > urlTrackEvents[nextUrlEvent].Item1)
          {
            nextEventTime = urlTrackEvents[nextUrlEvent].Item1;
          }

          var duration = nextEventTime - currentEventTime;
          if (duration > maxImpressionDuration)
          {
            duration = maxImpressionDuration;
          }

          yield return Tuple.Create(duration, this.topicEvents[i].Item2);
        }
      }
    }

    /// <summary>
    /// Storage class for ITrackingReport.
    /// </summary>
    private class TrackingReport : ITrackingReport
    {
      public TrackingReport(string code, string ip, DateTime startTime, TimeSpan duration, IEnumerable<ITopicReport> topics)
      {
        this.Code = code;
        this.Ip = ip;
        this.Topics = topics.ToArray();
        this.StartTime = startTime;
        this.Duration = duration;
      }

      /// <inheritdoc/>
      public DateTime StartTime { get; }

      /// <inheritdoc/>
      public TimeSpan Duration { get; }

      /// <inheritdoc/>
      public string Code { get; }

      /// <inheritdoc/>
      public string Ip { get; }

      /// <inheritdoc/>
      public IReadOnlyList<ITopicReport> Topics { get; }
    }

    /// <summary>
    /// Storage class for ITrackingReport.
    /// </summary>
    private class TopicReport : ITopicReport
    {
      public TopicReport(String title, String topicDetail, String url)
      {
        this.Title = title;
        this.Url = url;
        this.TopicDetail = topicDetail;
        this.ImpressionTimeSpan = TimeSpan.FromSeconds(0);
      }

      /// <inheritdoc/>
      public String Title { get; }

      /// <inheritdoc/>
      public String Url { get; }

      /// <inheritdoc/>
      public String TopicDetail { get; }

      /// <inheritdoc/>
      public TimeSpan ImpressionTimeSpan { get; set; }

      /// <inheritdoc/>
      public Int32 ImpressionCount { get; set; }

      /// <inheritdoc/>
      public Int32 DirectInvocationCount { get; set; }

      /// <inheritdoc/>
      public Int32 LastVisibleGeneration { get; set; }
    }
  }
}