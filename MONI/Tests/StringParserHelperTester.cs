using System;
using NUnit.Framework;
using MONI.Util;

namespace MONI.Tests
{
  [TestFixture]
  public class StringParserHelperTester
  {
    [Test]
    public void SplitWithIgnoreRegions_EmptyAndNullStringCases_DoNotCrash() {
      string s = null;
      s.SplitWithIgnoreRegions(new[] {','}, new IgnoreRegion('(', ')'));
      s = string.Empty;
      s.SplitWithIgnoreRegions(new[] {','}, new IgnoreRegion('(', ')'));
      s = "         ";
      s.SplitWithIgnoreRegions(new[] {','}, new IgnoreRegion('(', ')'));
    }

    [Test]
    public void SplitWithIgnoreRegions_NullSeparatorIgnoreregions_DoCrash() {
      Assert.Throws<ArgumentNullException>(() => "s".SplitWithIgnoreRegions(null));
      Assert.Throws<ArgumentNullException>(() => "s".SplitWithIgnoreRegions(new[] {','}));
    }

    [Test]
    public void SplitWithIgnoreRegions_EmptyRegions_SplitFine() {
      string s = ",()";
      var splitted = s.SplitWithIgnoreRegions(new[] {','}, new IgnoreRegion('(', ')'));
      CollectionAssert.AreEqual(new[] {string.Empty, "()"}, splitted);
    }

    [Test]
    public void SplitWithIgnoreRegions_EasyRegions_SplitFine() {
      string s = ",(,)";
      var splitted = s.SplitWithIgnoreRegions(new[] {','}, new IgnoreRegion('(', ')'));
      CollectionAssert.AreEqual(new[] {string.Empty, "(,)"}, splitted);
    }

    [Test]
    public void SplitWithIgnoreRegions_MultipleRegions_SplitFine() {
      string s = ",(,),{,}";
      var splitted = s.SplitWithIgnoreRegions(new[] {','}, new IgnoreRegion('(', ')'), new IgnoreRegion('{', '}'));
      CollectionAssert.AreEqual(new[] {string.Empty, "(,)", "{,}"}, splitted);
    }

    [Test]
    public void SplitWithIgnoreRegions_NestedRegions_SplitFine() {
      string s = ",(,{,}),{,}";
      var splitted = s.SplitWithIgnoreRegions(new[] {','}, new IgnoreRegion('(', ')'), new IgnoreRegion('{', '}'));
      CollectionAssert.AreEqual(new[] {string.Empty, "(,{,})", "{,}"}, splitted);
    }

    [Test]
    public void SplitOnFirst_EmptyString() {
      string s = string.Empty;
      var splitted = s.SplitOnFirst(",");
      Assert.AreEqual(string.Empty, splitted.Item1);
      Assert.AreEqual(string.Empty, splitted.Item2);
    }

    [Test]
    public void SplitOnFirst_OnlyOneOccurenceButOnlyLeftSide() {
      string s = "abc,";
      var splitted = s.SplitOnFirst(",");
      Assert.AreEqual("abc", splitted.Item1);
      Assert.AreEqual(string.Empty, splitted.Item2);
    }

    [Test]
    public void SplitOnFirst_OnlyOneOccurence() {
      string s = "abc,def";
      var splitted = s.SplitOnFirst(",");
      Assert.AreEqual("abc", splitted.Item1);
      Assert.AreEqual("def", splitted.Item2);
    }

    [Test]
    public void SplitOnFirst_MultipleOccurences() {
      string s = "abc,def,ghj";
      var splitted = s.SplitOnFirst(",");
      Assert.AreEqual("abc", splitted.Item1);
      Assert.AreEqual("def,ghj", splitted.Item2);
    }

    [Test]
    public void SplitOnFirst_OneOccurenceLongSeparator() {
      string s = "abc,def,,ghj";
      var splitted = s.SplitOnFirst(",,");
      Assert.AreEqual("abc,def", splitted.Item1);
      Assert.AreEqual("ghj", splitted.Item2);
    }


    [Test]
    public void SplitOnLast_EmptyString() {
      string s = string.Empty;
      var splitted = s.SplitOnLast(",");
      Assert.AreEqual(string.Empty, splitted.Item1);
      Assert.AreEqual(string.Empty, splitted.Item2);
    }

    [Test]
    public void SplitOnLast_OnlyOneOccurenceButOnlyLeftSide() {
      string s = "abc,";
      var splitted = s.SplitOnLast(",");
      Assert.AreEqual("abc", splitted.Item1);
      Assert.AreEqual(string.Empty, splitted.Item2);
    }

    [Test]
    public void SplitOnLast_OnlyOneOccurence() {
      string s = "abc,def";
      var splitted = s.SplitOnLast(",");
      Assert.AreEqual("abc", splitted.Item1);
      Assert.AreEqual("def", splitted.Item2);
    }

    [Test]
    public void SplitOnLast_MultipleOccurences() {
      string s = "abc,def,ghj";
      var splitted = s.SplitOnLast(",");
      Assert.AreEqual("abc,def", splitted.Item1);
      Assert.AreEqual("ghj", splitted.Item2);
    }

    [Test]
    public void SplitOnLast_OneOccurenceLongSeparator() {
      string s = "abc,def,,ghj";
      var splitted = s.SplitOnLast(",,");
      Assert.AreEqual("abc,def", splitted.Item1);
      Assert.AreEqual("ghj", splitted.Item2);
    }
  }
}