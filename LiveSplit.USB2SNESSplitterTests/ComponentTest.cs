using System;
using LiveSplit.Model;
using LiveSplit.UI.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static LiveSplit.UI.Components.USB2SNESComponent;

namespace LiveSplit.USB2SNESSplitterTests
{
    class FakeUSB2SnesW : USB2SnesW.USB2SnesW
    {
        internal FakeUSB2SnesW()
        {
            // the usb2snesw constructor connects on instantiation :scream_cat:
        }

    }

    [TestClass]
    public class ComponentTest
    {
        static string SPLIT_JSON = @"{
      ""name"": ""-TR Big Key"",
      ""address"": ""0xF366"",
      ""value"": ""0x8"",
      ""type"": ""bit""
}";
        [TestMethod]
        public void TestSplitDeserialization()
        {
            var s = new System.Web.Script.Serialization.JavaScriptSerializer();
            var split = s.Deserialize<Split>(SPLIT_JSON);
            Assert.AreEqual("-TR Big Key", split.name);
            Assert.AreEqual("0xF366", split.address);
            Assert.AreEqual(0xF366u, split.addressint);
            Assert.AreEqual(0b00001000u, split.valueint);
            Assert.AreEqual("bit", split.type);

            split.value = "0b00001000";
            Assert.ThrowsException<System.OverflowException>(() => split.valueint);
        }

        [TestMethod]
        public void TestCheckSplit()
        {
            LiveSplitState state = new LiveSplitState(null, null, null, null, null);
            USB2SNESComponent c = new USB2SNESComponent(state, new FakeUSB2SnesW());
            var s = new System.Web.Script.Serialization.JavaScriptSerializer();
            var split = s.Deserialize<Split>(SPLIT_JSON);
            Assert.IsTrue(split.check(0x8u, 0x8u));
        }
    }
}
