using MONI.ViewModels;
using NUnit.Framework;

namespace MONI.Tests
{
    [TestFixture]
    public class ProjectNumberTester
    {
        [TestCase("12345	A	B	C	0	Test --ALT --")]
        [TestCase("12345	ABCDE	BE	CE	0	Test")]
        [TestCase("12345	AE	BE	CE	1	Test")]
        public void Should_Parse_ProjectNumber_From_Line(string line)
        {
            Assert.IsTrue(new ProjectNumber().TryParse(line));
        }
    }
}