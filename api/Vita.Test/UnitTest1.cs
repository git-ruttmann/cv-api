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
        public void LoadDataFile()
        {
            var stream = new FileStream("/home/ruttmann/cvData/generalContent", FileMode.Open);
            var reader = new VitaStreamReader(stream, Encoding.UTF8);
            var items = reader.ReadEntries().ToArray();
            Assert.IsTrue(items.Length > 5);
            Assert.IsTrue(items.All(x => x.Codes.Any()));

            var collection = new VitaEntryCollection(items.Select(x => new VitaEntryForSerialization(x)));
            var text = JsonConvert.SerializeObject(collection, Formatting.Indented);
            text = JsonConvert.SerializeObject(items, Formatting.Indented);

            TestContext.WriteLine(text);
        }
    }
}
