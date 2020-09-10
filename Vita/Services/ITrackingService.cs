namespace ruttmann.vita.api
{
  using System;

  /// <summary>
  /// service for tracking activity
  /// </summary>
  public interface ITrackingService
  {
    /// <summary>
    /// Get an Observable for new reports
    /// </summary>
    IObservable<ITrackingReport> Reports { get; }

    /// <summary>
    /// Get the tracking session
    /// </summary>
    /// <param name="code">the login code</param>
    /// <param name="sessionId">the session identifier</param>
    /// <param name="ip">the remote ip</param>
    /// <returns>a session for tracking</returns>
    ITrackingSession GetSession(String code, String sessionId, String ip);
    
    /// <summary>
    /// publish the reports now (for unittesting). This is internally called by a timer.
    /// </summary>
    void PublishReports();
  }

  /// <summary>
  /// The time source for the tracking service.
  /// </summary>
  public interface ITimeSource
  {
      /// <summary>
      /// Gets the current time
      /// </summary>
      DateTime Now { get; }
  }

  /// <summary>
  /// a tracking session
  /// </summary>
  public interface ITrackingSession
  {
    /// <summary>
    /// Gets the login code
    /// </summary>
    String Code { get; }

    /// <summary>
    /// record the current visisble topics
    /// </summary>
    /// <param name="trackEvent">the tracking event</param>
    void RecordTopics(TrackTopicsEvent trackEvent);

    /// <summary>
    /// record a tracking event
    /// </summary>
    /// <param name="trackEvent">the tracking event</param>
    void RecordUrl(UrlTrackingEvent trackingEvent);

    /// <summary>
    /// Generate a tracking report from all recorded events
    /// </summary>
    /// <returns>a tracking report</returns>
    ITrackingReport GenerateReport();
  }
}
