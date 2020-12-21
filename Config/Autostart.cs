using System;

namespace LiveSplit.UI.Components
{
    internal class Autostart
    {
        public string active { get; set; }
        public string address { get; set; }
        public string value { get; set; }
        public string type { get; set; }

        public uint addressInt { get { return Convert.ToUInt32(address, 16); } }
        public uint valueInt { get { return Convert.ToUInt32(value, 16); } }
    }
}
