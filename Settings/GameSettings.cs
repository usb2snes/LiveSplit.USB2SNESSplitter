using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace LiveSplit.UI.Components
{
    internal class GameSettings
    {
        private const string CategoriesElementName = "Categories";
        private const string GameElementName = "Game";

        internal GameSettings(string name, string configFile)
        {
            Name = name;
            ConfigFile = configFile;
            CategoryMap = new Dictionary<string, CategorySettings>();
        }

        public string ConfigFile { get; set; }

        internal string Name { get; }
        internal Dictionary<string, CategorySettings> CategoryMap { get; }

        internal bool IsEmpty => string.IsNullOrWhiteSpace(ConfigFile) && CategoryMap.Values.All(c => c.IsEmpty);

        internal static GameSettings FromXml(XmlElement gameElement)
        {
            string name = SettingsHelper.ParseString(gameElement[nameof(Name)]);
            string configFile = SettingsHelper.ParseString(gameElement[nameof(ConfigFile)]);

            var gameSettings = new GameSettings(name, configFile);

            foreach (var categoryElement in gameElement[CategoriesElementName].ChildNodes.Cast<XmlElement>())
            {
                CategorySettings categorySettings = CategorySettings.FromXml(categoryElement);
                gameSettings.CategoryMap[categorySettings.Name] = categorySettings;
            }

            return gameSettings;
        }

        internal int ToXml(XmlDocument document, XmlElement parent)
        {
            var gameElement = document?.CreateElement(GameElementName);
            parent?.AppendChild(gameElement);

            return SettingsHelper.CreateSetting(document, gameElement, nameof(Name), Name) ^
                SettingsHelper.CreateSetting(document, gameElement, nameof(ConfigFile), ConfigFile) ^
                WriteCategoriesToXml(document, gameElement);
        }

        private int WriteCategoriesToXml(XmlDocument document, XmlElement parent)
        {
            var categoriesElement = document?.CreateElement(CategoriesElementName);
            parent?.AppendChild(categoriesElement);

            int result = 0;
            int count = 1;
            foreach (var categorySettings in CategoryMap.Values.Where(c => !c.IsEmpty))
            {
                result ^= count++ * categorySettings.ToXml(document, categoriesElement);
            }

            return result;
        }
    }
}
