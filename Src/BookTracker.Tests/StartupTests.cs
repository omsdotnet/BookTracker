using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace BookTracker.Tests
{
  [TestFixture(Category = "Unit", TestOf = typeof(Startup))]
  public class StartupTests
  {
    [Test]
    public void StartupTest()
    {
      var configuration = new Mock<IConfiguration>();
      var env = Mock.Of<IHostingEnvironment>();
      var logger = Mock.Of<ILogger<Startup>>();

      Assert.That(() =>
      {
        new Startup(configuration.Object, env, logger);
      }, Throws.Nothing);
    }
  }
}