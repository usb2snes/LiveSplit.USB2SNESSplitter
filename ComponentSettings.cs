using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using USB2SnesW;

namespace LiveSplit.UI.Components
{
    public partial class ComponentSettings : UserControl
    {

        public string Device { get; set; }
        public string ConfigFile { get; set; }
        public bool ResetSNES { get; set; }

        public ComponentSettings()
        {
            InitializeComponent();
            Device = "";
            ConfigFile = "";

            txtComPort.DataBindings.Add("Text", this, "Device", false, DataSourceUpdateMode.OnPropertyChanged);
            txtConfigFile.DataBindings.Add("Text", this, "ConfigFile", false, DataSourceUpdateMode.OnPropertyChanged);
            chkReset.DataBindings.Add("Checked", this, "ResetSNES", false, DataSourceUpdateMode.OnPropertyChanged);
        }
        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            Device = SettingsHelper.ParseString(element["Device"]);
            ConfigFile = SettingsHelper.ParseString(element["ConfigFile"]);
            ResetSNES = SettingsHelper.ParseBool(element["ResetSNES"]);    
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsNode(document, parent);
            return parent;
        }

        public int GetSettingsHashCode()
        {
            return CreateSettingsNode(null, null);
        }

        private int CreateSettingsNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "Version", "1.2") ^
            SettingsHelper.CreateSetting(document, parent, "Device", Device) ^
            SettingsHelper.CreateSetting(document, parent, "ConfigFile", ConfigFile) ^
            SettingsHelper.CreateSetting(document, parent, "ResetSNES", ResetSNES);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "JSON Files|*.json";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                txtConfigFile.Text = ofd.FileName;
            }
        }

        private async void btnDetect_Click(object sender, EventArgs e)
        {
            USB2SnesW.USB2SnesW usb = new USB2SnesW.USB2SnesW();
            await usb.Connect();
            
            if (usb.Connected())
            {
                List<String> devices;
                devices = await usb.GetDevices();
                if (devices.Count > 0)
                    txtComPort.Text = devices[0];
                return;
            }
            MessageBox.Show("Could not auto-detect usb2snes compatible device, make sure it's connected and QUsb2Snes is running");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }

}
