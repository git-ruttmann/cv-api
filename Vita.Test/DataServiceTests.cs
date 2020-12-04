namespace Vita.Test
{
  using System;
  using System.Linq;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using ruttmann.vita.api;

  /// <summary>
  /// Test the data service
  /// </summary>
  [TestClass]
  public class DataServiceTests
  {
    /// <summary>
    /// load the contents for code xx
    /// </summary>
    [TestMethod]
    public void LoadXx()
    {
      var dataService = VitaDataService.CreateMockedService(Scenario1(), new[] { "general", "codes" });
      var items = dataService.GetEntriesForCode("xx");

      Assert.AreEqual(12, items.Entries.Length);
      Assert.IsTrue(items.Entries.Any(x => x.Title == "Strength2"));
      Assert.IsTrue(items.Entries.Any(x => x.Title == "Not for yy"));
    }

    /// <summary>
    /// Verify the use of global attributes.
    /// </summary>
    [TestMethod]
    public void VerifyGlobalAttributes()
    {
      var dataService = VitaDataService.CreateMockedService(Scenario1(), new[] { "general", "codes" });
      var items = dataService.GetEntriesForCode("xx").Entries;

      var introduction = items.Single(x => x.Title == "Welcome");
      Assert.IsTrue(introduction.Attributes.Contains("English"), "Must derive global attribute");

      var strength1 = items.Single(x => x.Title == "Strength1");
      Assert.IsTrue(strength1.Attributes.Contains("Short"), "Must derive global attribute");

      var strength2 = items.Single(x => x.Title == "Strength2");
      Assert.IsTrue(strength2.Attributes.Contains("Medium"), "Must use the attribute");
      Assert.IsFalse(strength2.Attributes.Contains("Short"), "Must not derive the global attribute");
      Assert.IsTrue(strength2.Attributes.Contains("German"), "Language isn't configured, must default to full language");
      Assert.IsTrue(strength2.Attributes.Contains("English"), "Language isn't configured, must default to full language");

      Assert.IsTrue(items.Any(x => x.Title == "Not for yy"));
    }

    /// <summary>
    /// load the contents for code yy
    /// </summary>
    [TestMethod]
    public void LoadYy()
    {
      var dataService = VitaDataService.CreateMockedService(Scenario1(), new[] { "general", "codes" });
      var items = dataService.GetEntriesForCode("yy");

      var titles = items.Entries.Select(x => x.Title).ToArray();

      Assert.IsTrue(items.Entries.Any(x => x.Title == "For yy only"));
      Assert.IsFalse(items.Entries.Any(x => x.Title == "Not for yy"));
      Assert.AreEqual(10, items.Entries.Length);
    }

    /// <summary>
    /// Test topics can be deselected by a group.
    /// </summary>
    [TestMethod]
    public void TestGroupNegation()
    {
      var dataService = VitaDataService.CreateMockedService(Scenario1(), new[] { "general", "codes" });
      var itemZ = dataService.GetEntriesForCode("zz").Entries.Single(x => x.Title == "Bullets and text");
      var itemY = dataService.GetEntriesForCode("yy").Entries.Single(x => x.Title == "Bullets and text");

      Assert.IsFalse(itemY.Lines.Any(x => x == "Initial text for the expert"));
      Assert.IsTrue(itemZ.Lines.Any(x => x == "Initial text for the expert"));
    }

    /// <summary>
    /// Just load the contents
    /// </summary>
    [TestMethod]
    public void TestValidCodes()
    {
      var dataservice = VitaDataService.CreateMockedService(Scenario1(), new[] { "general", "codes" });

      Assert.IsTrue(dataservice.IsValidCode("xx"));
      Assert.IsTrue(dataservice.IsValidCode("yy"));
      Assert.IsTrue(dataservice.IsValidCode("zz"));
      Assert.IsFalse(dataservice.IsValidCode("abc"));
      Assert.IsFalse(dataservice.IsValidCode("ab"));
    }

    /// <summary>
    /// Query the animation for a code that doesn't have one
    /// </summary>
    [TestMethod]
    public void TestMissingAnimation()
    {
      var dataservice = VitaDataService.CreateMockedService(Scenario1(), new[] { "general", "codes" });

      Assert.IsTrue(dataservice.IsValidCode("xx"));
      Assert.AreEqual(0, dataservice.GetCustomAnimationForCode("xx").Length);
    }

    /// <summary>
    /// Query the animation for a code with animation
    /// </summary>
    [TestMethod]
    public void TestAnimation()
    {
      var dataservice = VitaDataService.CreateMockedService(Scenario1(), new[] { "general", "codes" });

      Assert.IsTrue(dataservice.IsValidCode("xg"));
      Assert.AreEqual("flyinGoogle", dataservice.GetCustomAnimationForCode("xg"));
    }

    [TestMethod]
    public void TestAuthCookies()
    {
      var timeMock = new MockTimeSource();
      var dataService = VitaDataService.CreateMockedService(Scenario1(), new[] { "general", "codes" });
      var authService = new VitaAuthService(dataService, timeMock);

      Assert.IsTrue(authService.IsValidCode("xx", "xx", out var session), "Must authenticate");
      Assert.IsTrue(authService.IsValidCookie(session.Cookie, out var otherSession));
      Assert.ReferenceEquals(session, otherSession);

      Assert.IsFalse(authService.IsValidCookie(session.Cookie + "A", out var badSession), "different cookie must be invalid");
      Assert.IsNull(badSession, "invalid cookie must return null session");

      timeMock.Now += TimeSpan.FromHours(4);
      Assert.IsFalse(authService.IsValidCookie(session.Cookie, out var timedOutSession), "Cookie may not live longer than 2 hours");
      Assert.IsNull(timedOutSession, "timed out cookie must return null session");
    }

    [TestMethod]
    public void TestIntermediateIncludes()
    {
      var mock = new FileSystemMock();
      mock.AddFile("codes", "xx: all");
      mock.AddFile("general", "##include: l1.txt");
      mock.AddFile("l1.txt", @"
#code: all
##include: l2.txt
##project: P1
P1 text

##include: l3.txt
##include: l4.txt");
      mock.AddFile("l2.txt", @"
#code: all
##project: P2
P2 text");
      mock.AddFile("l3.txt", @"
#code: all
##project: P3
P3 text");
      mock.AddFile("l4.txt", @"
#code: all
##project: P4
P4 text");

      var dataService = VitaDataService.CreateMockedService(mock, new[] { "general", "codes" });
      var entries = dataService.GetEntriesForCode("xx").Entries;

      Assert.AreEqual(1, entries.Count(x => x.Title == "P1"));
      Assert.AreEqual(1, entries.Count(x => x.Title == "P2"));
      Assert.AreEqual(1, entries.Count(x => x.Title == "P3"));
      Assert.AreEqual(1, entries.Count(x => x.Title == "P4"));
    }

    /// <summary>
    /// Provide some fake files
    /// </summary>
    /// <returns>A file system with content</returns>
    public static IFileSystem Scenario1()
    {
      var mock = new FileSystemMock();
      var codes = @"
xx: all
xg: all
yy: all
zz: all dotnet expert
";
      mock.AddFile("codes", codes);

      var person = @"
#code: all

##person: Passion
#attributes: english, short, medium, long

Text about Passion

##person: Bullets only
#attributes: english, short
- The first bullet
- The second bullet

##person: Bullets and text
#attributes: english, short
#code: all -expert
Initial text
- The first bullet
- The second bullet
Trailing text

##person: Bullets and text
#attributes: english, short
#code: expert
Initial text for the expert
- The first bullet
- The second bullet
Trailing text

##person: Text and links
#attributes: english, short
Some initial text
- Bullet one
- [""bullet link"", ""https://my.mycv.com/bullettarget""]
- More bullets
[""normal link"", ""https://my.mycv.com/normaltarget""]

##person: For yy only
#attributes: english, short
#code: yy

##person: For all explicitly
#attributes: english, short
#code: all

##person: Not for yy
#attributes: english, short
#code: -yy
      ";

      mock.AddFile("person.txt", person);

      var introduction = @"
#attributes: english

##introduction: Herzlich willkommen.
#attributes: german
#code: yy

Hallo yy, Sie sind deutsch.

##introduction: Herzlich willkommen.
#attributes: german
#code: xg

Hallo xg, hier ist eine spezielle animation.
animation:flyinGoogle

##introduction: Herzlich willkommen.
#code: dotnet

Hello, you're from the group dotnet.

##introduction: Welcome
#code: xx

Hello xx, you're english.
      ";
      mock.AddFile("introduction.txt", introduction);

      var general = @"
#code: all
#attributes: short

##include: introduction.txt
##include: person.txt

##strength: Strength1

Text for strength one.

##strength: Strength2
#attributes: medium
#code: xx

Text for strength two.

##strength: Strength3
#attributes: long
#code: -yy
#code: -zz

Text for strength three.

##technology: Tech1

Text for tech1

##interest: Motivation

I'm motivated. Yeah.
        ";

      mock.AddFile("general", general);

      return mock;
    }

    /// <summary>
    /// Mock the current time
    /// </summary>
    private class MockTimeSource : ITimeSource
    {
      /// <inheritdoc/>
      public DateTime Now { get; set; } = DateTime.UtcNow;
    }
  }
}