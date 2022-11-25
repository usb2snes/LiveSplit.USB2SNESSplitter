using System;
using System.Collections.Generic;

namespace LiveSplit.UI.Components
{
    internal class Autostart : Split
    {
        public string active { get; set; }

        public Autostart()
        {
            name = "Autostart";
        }
        public Split GetSplit()
        {
            return this;
        }
    }
}
