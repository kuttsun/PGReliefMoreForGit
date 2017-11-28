using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PGReliefMoreForGit.Models.Update;

namespace PGReliefMoreForGit.Tests
{
    [TestClass]
    public class UnitTest1
    {
        Update update = Update.Instance;

        [TestMethod]
        public void GetLatestVersionTest()
        {
            update.PreRelease = false;
            //Assert.AreEqual("0.3.0", update.GetLatestVersionAsync().Result);

            update.PreRelease = true;
            //Assert.AreEqual("0.4.0", update.GetLatestVersionAsync().Result);
        }
    }
}
