using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace LiveSplit.UI.Components
{
    internal class ConfigFileJsonConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes { get; } = new[] { typeof(Game) };

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            var game = new Game
            {
                name = serializer.ConvertToType<string>(LookupKeys(dictionary, "name", "game")),
                autostart = serializer.ConvertToType<Autostart>(LookupKeys(dictionary, "autostart")),
                igt = serializer.ConvertToType<InGameTime>(LookupKeys(dictionary, "igt")),
                alias = serializer.ConvertToType<Dictionary<string, string>>(LookupKeys(dictionary, "alias")),
                definitions = serializer.ConvertToType<List<Split>>(LookupKeys(dictionary, "definitions")),
            };
            game.autostart.GetSplit().validate();
            if (game.definitions != null)
            {
                foreach (var split in game.definitions)
                {
                    split.validate();
                }
            }
            return game;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            Game game = obj as Game;
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["game"] = game.name;
            if (game.autostart != null)
                result["autostart"] = game.autostart;
            if (game.igt != null)
                result["igt"] = game.igt;
            if (game.alias != null)
                result["alias"] = game.alias;
            if (game.definitions != null && game.definitions.Count > 0)
                result["definitions"] = game.definitions;
            return result;
        }

        private static object LookupKeys(IDictionary<string, object> dictionary, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (dictionary.TryGetValue(key, out object value))
                {
                    return value;
                }
            }

            return null;
        }
    }
}
