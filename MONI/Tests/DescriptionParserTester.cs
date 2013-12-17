using System;
using System.Collections.Generic;
using MONI.Data;
using MONI.Parser;
using NUnit.Framework;

namespace MONI.Tests
{
  [TestFixture]
  public class DescriptionParserTester
  {
    [Test]
    public void DP_Null_DoNothing() {
      var res = DescriptionParser.ParseDescription(null);
      Assert.IsEmpty(res.BeforeDescription);
      Assert.IsEmpty(res.Description);
    }

    [Test]
    public void DP_StringEmpty_DoNothing() {
      var res = DescriptionParser.ParseDescription(string.Empty);
      Assert.IsEmpty(res.BeforeDescription);
      Assert.IsEmpty(res.Description);
    }

    [Test]
    public void DP_NoDesc_Parse() {
      var res = DescriptionParser.ParseDescription("11111-111");
      Assert.AreEqual("11111-111", res.BeforeDescription);
      Assert.IsEmpty(res.Description);
    }

    [Test]
    public void DP_NormalDesc_Parse() {
      var res = DescriptionParser.ParseDescription("11111-111(abc)");
      Assert.AreEqual("11111-111", res.BeforeDescription);
      Assert.AreEqual("abc", res.Description);
    }

    [Test]
    public void DP_DescWithNormalBrackets_Parse() {
      var res = DescriptionParser.ParseDescription("11111-111(abc(def)ghi)");
      Assert.AreEqual("11111-111", res.BeforeDescription);
      Assert.AreEqual("abc(def)ghi", res.Description);
    }

    [Test]
    public void DP_AppendDesc_Parse() {
      var res = DescriptionParser.ParseDescription("11111-111(+abc)");
      Assert.AreEqual("11111-111", res.BeforeDescription);
      Assert.AreEqual("abc", res.Description);
    }

    [Test]
    public void DP_AppendWithNormalBrackets_Parse() {
      var res = DescriptionParser.ParseDescription("11111-111(+abc(def)ghi)");
      Assert.AreEqual("11111-111", res.BeforeDescription);
      Assert.AreEqual("abc(def)ghi", res.Description);
    }
 }
}