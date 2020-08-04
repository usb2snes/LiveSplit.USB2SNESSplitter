using System;
using System.Reflection;
using LiveSplit.Model;
using LiveSplit.UI.Components;

[assembly: ComponentFactory(typeof(Factory))]

namespace LiveSplit.UI.Components
{
    public class Factory : IComponentFactory
    {
        public string ComponentName => "USB2SNES Auto Splitter";
        public string Description => "Uses the SD2SNES USB2SNES firmware to monitor RAM for auto splitting.";
        public ComponentCategory Category => ComponentCategory.Control;
        public Version Version => Version.Parse("1.0.0");

        public string UpdateName => ComponentName;
        public string UpdateURL => "";
        public string XMLURL => "";

        public IComponent Create(LiveSplitState state) => new USB2SNESComponent(state);

        static Factory()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string resourceName = "LiveSplit." + new AssemblyName(args.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] assemblyData = new byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }

                return null;
            };
        }
    }
}
