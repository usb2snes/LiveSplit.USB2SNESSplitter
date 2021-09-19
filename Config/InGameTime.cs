using System;

namespace LiveSplit.UI.Components
{
    internal class InGameTime
    {
        public string active { get; set; }
        public string framesAddress { get; set; }
        public string secondsAddress { get; set; }
        public string minutesAddress { get; set; }
        public string hoursAddress { get; set; }

        public uint framesAddressInt { get { return String.IsNullOrEmpty(framesAddress) ? 0 : Convert.ToUInt32(framesAddress, 16); } }
        public uint secondsAddressInt { get { return String.IsNullOrEmpty(secondsAddress) ? 0 : Convert.ToUInt32(secondsAddress, 16); } }
        public uint minutesAddressInt { get { return String.IsNullOrEmpty(minutesAddress) ? 0 : Convert.ToUInt32(minutesAddress, 16); } }
        public uint hoursAddressInt { get { return String.IsNullOrEmpty(hoursAddress) ? 0 : Convert.ToUInt32(hoursAddress, 16); } }
    }
}
