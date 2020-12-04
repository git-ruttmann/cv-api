namespace ruttmann.vita.api
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public interface ITopicReport
  {
      /// <summary>
      /// Gets the topic title
      /// </summary>
      String Title { get; }

      /// <summary>
      /// Gets the url of the topic
      /// </summary>
      String Url { get; }

      /// <summary>
      /// Gets the detail level (duration)
      /// </summary>
      String TopicDetail { get; }

      /// <summary>
      /// Gets the total time, this topic was visible
      /// </summary>
      TimeSpan ImpressionTimeSpan { get; }
      
      /// <summary>
      /// Gets the number of times, this item was visible again (multiple scroll events is one count)
      /// </summary>
      Int32 ImpressionCount { get; }

      /// <summary>
      /// Gets the number of times, this item was clicked
      /// </summary>
      Int32 DirectInvocationCount { get; }
  }

  public interface ITrackingReport
  {
    DateTime StartTime { get; }
    TimeSpan Duration { get; }
    String Code { get; }
    String Name { get; }
    String Ip { get; }
    IReadOnlyList<ITopicReport> Topics { get;}
    IReadOnlyList<string> ClickedLinks { get; }
  }
}