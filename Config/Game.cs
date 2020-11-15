using System.Collections.Generic;

namespace LiveSplit.UI.Components
{
    internal class Game
    {
        public string name { get; set; }
        public Autostart autostart { get; set; }
        public Dictionary<string, string> alias { get; set; }
        public List<Split> definitions { get; set; }
    }
}
