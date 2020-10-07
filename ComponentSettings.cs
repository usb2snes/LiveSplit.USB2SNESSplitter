using LiveSplit.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class ComponentSettings : UserControl
    {

        public string Device { get; set; } = string.Empty;
        public string ConfigFile { get; set; } = string.Empty;
        public bool ResetSNES { get; set; } = false;
        public bool ShowStatusMessage { get; set; } = true;

        public ComponentSettings()
        {
            InitializeComponent();

            txtDevice.DataBindings.Add(nameof(TextBox.Text), this, nameof(Device), false, DataSourceUpdateMode.OnPropertyChanged);
            txtConfigFile.DataBindings.Add(nameof(TextBox.Text), this, nameof(ConfigFile), false, DataSourceUpdateMode.OnPropertyChanged);
            chkReset.DataBindings.Add(nameof(CheckBox.Checked), this, nameof(ResetSNES), false, DataSourceUpdateMode.OnPropertyChanged);
            chkStatus.DataBindings.Add(nameof(CheckBox.Checked), this, nameof(ShowStatusMessage), false, DataSourceUpdateMode.OnPropertyChanged);

            errorIcon.Image = SystemIcons.Error.ToBitmap();
        }
        public void SetSettings(XmlNode node)
        {
            var settingsElements = (XmlElement)node;
            var versionElement = settingsElements["Version"];
            int version = versionElement?.InnerText == "1.2" ? 1 : SettingsHelper.ParseInt(versionElement);
            switch (version)
            {
                case 1:
                    LoadSettings_1(settingsElements);
                    break;

                case 2:
                    LoadSettings_2(settingsElements);
                    break;

                default:
                    Log.Error($"Tried to load Usb2SnesSplitter settings with unknown version: {version}");
                    break;
            }
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

        internal void SetError(string message)
        {
            errorMessage.Text = message;
            errorPanel.Visible = !string.IsNullOrEmpty(message);
        }

        private int CreateSettingsNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "Version", 2) ^
            SettingsHelper.CreateSetting(document, parent, nameof(Device), Device) ^
            SettingsHelper.CreateSetting(document, parent, nameof(ConfigFile), ConfigFile) ^
            SettingsHelper.CreateSetting(document, parent, nameof(ResetSNES), ResetSNES) ^
            SettingsHelper.CreateSetting(document, parent, nameof(ShowStatusMessage), ShowStatusMessage);
        }

        private void LoadSettings_1(XmlElement element)
        {
            Device = SettingsHelper.ParseString(element[nameof(Device)]);
            ConfigFile = SettingsHelper.ParseString(element[nameof(ConfigFile)]);
            ResetSNES = SettingsHelper.ParseBool(element[nameof(ResetSNES)]);
            ShowStatusMessage = false;
        }

        private void LoadSettings_2(XmlElement element)
        {
            Device = SettingsHelper.ParseString(element[nameof(Device)]);
            ConfigFile = SettingsHelper.ParseString(element[nameof(ConfigFile)]);
            ResetSNES = SettingsHelper.ParseBool(element[nameof(ResetSNES)]);
            ShowStatusMessage = SettingsHelper.ParseBool(element[nameof(ShowStatusMessage)]);
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
                    txtDevice.Text = devices[0];
                return;
            }
            MessageBox.Show("Could not auto-detect usb2snes compatible device, make sure it's connected and QUsb2Snes is running");
        }
    }
}
