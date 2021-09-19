using System;
using System.Collections.Generic;

namespace LiveSplit.UI.Components
{
    internal class Autostart
    {
        public string active { get; set; }
        public string address { get; set; }
        public string value { get; set; }
        public string type { get; set; }
        public List<Split> more { get; set; }
        public List<Split> next { get; set; }

        public Split GetSplit()
        {
            if (split == null)
            {
                split = new Split();
                split.address = address;
                split.value = value;
                split.type = type;
                split.more = more;
                split.next = next;
            }
            return split;
        }

        private Split split = null;
    }
}
