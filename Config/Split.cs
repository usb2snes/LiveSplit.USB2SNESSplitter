using System;
using System.Collections.Generic;

namespace LiveSplit.UI.Components
{
    internal class Split
    {
        public string name { get; set; }
        public string address { get; set; }
        public string value { get; set; }
        public string type { get; set; }
        public List<Split> more { get; set; }
        public List<Split> next { get; set; }
        public int posToCheck { get; set; } = 0;

        public uint addressInt { get { return Convert.ToUInt32(address, 16); } }
        public uint valueInt { get { return Convert.ToUInt32(value, 16); } }

        public bool check(uint value, uint word)
        {
            bool ret = false;
            switch (this.type)
            {
                case "bit":
                    if ((value & this.valueInt) != 0) { ret = true; }
                    break;
                case "eq":
                    if (value == this.valueInt) { ret = true; }
                    break;
                case "gt":
                    if (value > this.valueInt) { ret = true; }
                    break;
                case "lt":
                    if (value < this.valueInt) { ret = true; }
                    break;
                case "gte":
                    if (value >= this.valueInt) { ret = true; }
                    break;
                case "lte":
                    if (value <= this.valueInt) { ret = true; }
                    break;
                case "wbit":
                    if ((word & this.valueInt) != 0) { ret = true; }
                    break;
                case "weq":
                    if (word == this.valueInt) { ret = true; }
                    break;
                case "wgt":
                    if (word > this.valueInt) { ret = true; }
                    break;
                case "wlt":
                    if (word < this.valueInt) { ret = true; }
                    break;
                case "wgte":
                    if (word >= this.valueInt) { ret = true; }
                    break;
                case "wlte":
                    if (word <= this.valueInt) { ret = true; }
                    break;
            }
            return ret;
        }

        public void validate()
        {
            if (more != null)
            {
                foreach (var moreSplit in more)
                {
                    if (moreSplit.more != null)
                    {
                        throw new NotSupportedException("Nested 'more' splits are not supported");
                    }
                    if (moreSplit.next != null)
                    {
                        throw new NotSupportedException("Nested 'next' splits are not supported");
                    }
                }
            }
            if (next != null)
            {
                foreach (var nextSplit in next)
                {
                    if (nextSplit.next != null)
                    {
                        throw new NotSupportedException("Nested 'next' splits are not supported");
                    }
                    nextSplit.validate();
                }
            }
        }
    }
}
