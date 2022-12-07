using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace LiveSplit.UI.Components
{
    internal class Game
    {
        private static readonly JavaScriptSerializer _serializer;

        public string name { get; set; }
        public Autostart autostart { get; set; }
        public InGameTime igt { get; set; }
        public Dictionary<string, string> alias { get; set; }
        public List<Split> definitions { get; set; }

        static Game()
        {
            _serializer = new JavaScriptSerializer();
            _serializer.RegisterConverters(new[] { new ConfigFileJsonConverter() });
        }
        public string toJson()
        {
            return _serializer.Serialize(this);

        }
        internal static Game FromJSON(string json)
        {
            return _serializer.Deserialize<Game>(json);
        }

        internal string GetAutosplitNameFromSegmentName(string segmentName)
        {
            if (definitions.Any(split => split.name == segmentName))
            {
                // We found an autosplit with the same name as the segment
                return segmentName;
            }
            else if (alias != null && alias.TryGetValue(segmentName, out string autosplitName))
            {
                // We found an autosplit alias with the same name as the segment
                return autosplitName;
            }

            return string.Empty;
        }
    }
}
