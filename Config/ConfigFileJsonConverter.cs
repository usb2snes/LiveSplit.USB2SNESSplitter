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
            return new Game
            {
                name = serializer.ConvertToType<string>(LookupKeys(dictionary, "name", "game")),
                autostart = serializer.ConvertToType<Autostart>(LookupKeys(dictionary, "autostart")),
                alias = serializer.ConvertToType<Dictionary<string, string>>(LookupKeys(dictionary, "alias")),
                definitions = serializer.ConvertToType<List<Split>>(LookupKeys(dictionary, "definitions")),
            };
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
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
