namespace Vita.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using ruttmann.vita.api;

    [TestClass]
    public class UnitTest1
    {

    public TestContext TestContext { get; set; } 

        [TestMethod]
        public void CollectStats()
        {
            var dataMock = new DataServiceMock();
            var timeMock = new MockedTimeSource();

            var authService = new VitaAuthService(dataMock);
            var trackService = new TrackingService(timeMock);

            ITrackingReport report = null;
            trackService.Reports.Subscribe(x => report = x);

            ITrackingSession session = SessionTrackingScenarioOne(timeMock, trackService);
            trackService.PublishReports();

            Assert.IsNotNull(report);
            Assert.AreEqual(report.Topics.Count(), 3);
            Assert.IsTrue(report.Topics.All(x => x.ImpressionCount == 1));
            Assert.AreEqual(report.Topics[0].Url, "person");
            Assert.AreEqual(report.Topics[0].TopicDetail, "S");
            Assert.AreEqual(report.Topics[0].ImpressionTimeSpan, TimeSpan.FromSeconds(20 + 12 + 0));
            Assert.AreEqual(report.Topics[1].ImpressionTimeSpan, TimeSpan.FromSeconds(20 + 60 + 60));
            Assert.AreEqual(report.Topics[2].ImpressionTimeSpan, TimeSpan.FromSeconds(2 + 18 + 42));
        }

        private static ITrackingSession SessionTrackingScenarioOne(MockedTimeSource timeMock, TrackingService trackService)
        {
            var session = trackService.GetSession("Test", Guid.NewGuid().ToString(), "1.1.1.1");

            var startDate = new DateTime(2020, 2, 2, 12, 0, 13);

            var track1 = new TrackTopic[]
            {
                new TrackTopic { Topic = "Person 1", Start = 0, End = 1 },
                new TrackTopic { Topic = "Person 2", Start = 0, End = 1 },
                new TrackTopic { Topic = "Person 3", Start = 0, End = 0.1 },
            };
            timeMock.Now = startDate + TimeSpan.FromSeconds(10);
            session.RecordTopics(new TrackTopicsEvent("person", "S", track1));
            var track2 = new TrackTopic[]
            {
                new TrackTopic { Topic = "Person 1", Start = 0.8, End = 1 },
                new TrackTopic { Topic = "Person 2", Start = 0, End = 1 },
                new TrackTopic { Topic = "Person 3", Start = 0, End = 0.3 },
            };
            timeMock.Now = startDate + TimeSpan.FromSeconds(30);
            session.RecordTopics(new TrackTopicsEvent("person", "S", track2));
            var track3 = new TrackTopic[]
            {
                new TrackTopic { Topic = "Person 2", Start = 0, End = 1 },
                new TrackTopic { Topic = "Person 3", Start = 0, End = 0.7 },
            };
            timeMock.Now = startDate + TimeSpan.FromSeconds(300);
            session.RecordTopics(new TrackTopicsEvent("person", "S", track3));
            return session;
        }

        [TestMethod]
        [Ignore]
        public void LoadDataFile()
        {
            var stream = new FileStream("/home/ruttmann/cvData/generalContent", FileMode.Open);
            var reader = new VitaStreamReader(new DiskFileSystem(), stream, Encoding.UTF8);
            var items = reader.ReadEntries().ToArray();
            Assert.IsTrue(items.Length > 5);
            Assert.IsTrue(items.All(x => x.Codes.Any()));

            var collection = new VitaEntryCollection(items.Select(x => new VitaEntryForSerialization(x)));
            var text = JsonConvert.SerializeObject(collection, Formatting.Indented);
            text = JsonConvert.SerializeObject(items, Formatting.Indented);

            TestContext.WriteLine(text);
        }

        private class MockedTimeSource : ITimeSource
        {
            public DateTime Now { get; set; }
        }
    }
}
