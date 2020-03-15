using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dependency;
using System.Diagnostics;

namespace Test {

[TestClass]
public class TestLocalGitInstall {

    LocalGitInstall install = new LocalGitInstall();

    [TestMethod]
    public void TestUsable() {
        var installed = install.IsUsable;
        Assert.AreEqual(true, installed);
    }

    [TestMethod]
    public void TestCloneAndCheckout() {
        install.Clone(new Uri("https://github.com/githubtraining/hellogitworld.git"), ".trash");
        install.Checkout(".trash", "master");
    }
}

}