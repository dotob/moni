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
  }
}