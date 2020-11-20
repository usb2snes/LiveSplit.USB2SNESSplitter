using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace LiveSplit.UI.Components
{
    internal class CategorySettings
    {
        private const string AutosplitNameElementName = "AutosplitName";
        private const string CategoryElementName = "Category";
        private const string SegmentNameElementName = "SegmentName";
        private const string SplitElementName = "Split";
        private const string SplitsElementName = "Splits";

        internal CategorySettings(string name)
        {
            Name = name;
            SplitMap = new Dictionary<string, string>();
        }

        internal string Name { get; }
        internal Dictionary<string /*segment name*/, string /*autosplit name*/> SplitMap { get; }

        internal bool IsEmpty => SplitMap.Values.All(s => string.IsNullOrWhiteSpace(s));

        internal static CategorySettings FromXml(XmlElement categoryElement)
        {
            string name = SettingsHelper.ParseString(categoryElement[nameof(Name)]);
            var categorySettings = new CategorySettings(name);
            foreach (var splitElement in categoryElement[SplitsElementName].ChildNodes.Cast<XmlElement>())
            {
                string segmentName = SettingsHelper.ParseString(splitElement[SegmentNameElementName]);
                string autosplitName = SettingsHelper.ParseString(splitElement[AutosplitNameElementName]);
                categorySettings.SplitMap[segmentName] = autosplitName;
            }

            return categorySettings;
        }


        internal int ToXml(XmlDocument document, XmlElement parent)
        {
            var categoryElement = document?.CreateElement(CategoryElementName);
            parent?.AppendChild(categoryElement);

            return SettingsHelper.CreateSetting(document, categoryElement, nameof(Name), Name) ^
                WriteSplitsToXml(document, categoryElement);
        }

        private int WriteSplitsToXml(XmlDocument document, XmlElement parent)
        {
            var splitsElement = document?.CreateElement(SplitsElementName);
            parent?.AppendChild(splitsElement);

            int result = 0;
            int count = 1;
            foreach (var split in SplitMap)
            {
                var splitElement = document?.CreateElement(SplitElementName);
                splitsElement?.AppendChild(splitElement);

                result ^= count++ *
                (
                    SettingsHelper.CreateSetting(document, splitElement, SegmentNameElementName, split.Key) ^
                    SettingsHelper.CreateSetting(document, splitElement, AutosplitNameElementName, split.Value)
                );
            }

            return result;
        }
    }
}
