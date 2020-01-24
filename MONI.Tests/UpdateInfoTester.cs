using System.Reflection;
using System.Threading.Tasks;
using MONI.Data;
using MONI.Util;
using NUnit.Framework;
using Newtonsoft.Json;

namespace MONI.Tests
{
    [TestFixture]
    public class UpdateInfoTester
    {
        [Test]
        public async Task Read_Infos()
        {
            var reader = new AssemblyTextFileReader(Assembly.GetExecutingAssembly());
            var jsonTxt = await reader.ReadFileAsync(@"updateinfotest.json.txt");
            var uiArray = JsonConvert.DeserializeObject<UpdateInfo[]>(jsonTxt);
            CollectionAssert.IsNotEmpty(uiArray);
            var ui = uiArray[0];
            Assert.AreEqual(ui.Version, "1.1.1.1");
            Assert.AreEqual(ui.Changes[0], "eins");
            Assert.AreEqual(ui.Changes[1], "zwei");
            Assert.AreEqual(ui.DownLoadURL, "http://mtools/MONI.exe");
        }
    }
}