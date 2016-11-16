using MONI.Data;
using NUnit.Framework;
using Newtonsoft.Json;

namespace MONI.Tests
{
    public class UpdateInfoTester
    {
        [Test]
        public void Read_Infos()
        {
            var uiArray = JsonConvert.DeserializeObject<UpdateInfo[]>(Properties.Resources.UpdateInfoTest_json);
            CollectionAssert.IsNotEmpty(uiArray);
            var ui = uiArray[0];
            Assert.AreEqual(ui.Version, "1.1.1.1");
            Assert.AreEqual(ui.Changes[0], "eins");
            Assert.AreEqual(ui.Changes[1], "zwei");
            Assert.AreEqual(ui.DownLoadURL, "http://mtools/MONI.exe");
        }
    }
}