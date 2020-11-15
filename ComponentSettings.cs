using LiveSplit.Model;
using LiveSplit.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class ComponentSettings : UserControl
    {
        private class AutosplitSelection
        {
            internal AutosplitSelection(string autosplitName)
            {
                AutosplitName = autosplitName;
            }

            public string AutosplitName { get; set; }
        }

        private const string SegmentNameElementName = "SegmentName";
        private const string SplitElementName = "Split";
        private const string SplitsElementName = "Splits";
        private static readonly IEnumerable<string> _emptySplitChoices = new List<string> { string.Empty };

        private readonly LiveSplitState _state;
        private string _previousConfigJson = null;
        private IEnumerable<string> _splitChoices = _emptySplitChoices;
        private List<string> _cachedSplits = null;
        private bool _settingsChanged = false;
        private Dictionary<ISegment, AutosplitSelection> _segmentMap = new Dictionary<ISegment, AutosplitSelection>();
        private Dictionary<string /*segment name*/, string /*autosplit name*/> _segmentNameMap = new Dictionary<string, string>();

        public string Device { get; set; } = string.Empty;
        public string ConfigFile { get; set; } = string.Empty;
        public bool ResetSNES { get; set; } = false;
        public bool ShowStatusMessage { get; set; } = true;

        internal Game Config { get; private set; }

        private IEnumerable<string> SplitChoices
        {
            get => _splitChoices;
            set
            {
                if (!_splitChoices.SequenceEqual(value))
                {
                    _splitChoices = value;
                    foreach (var comboBox in splitsPanel.Controls.OfType<ComboBox>())
                    {
                        string previousSelection = (string)comboBox.SelectedItem;
                        ((BindingSource)comboBox.DataSource).DataSource = SplitChoices;
                        if (SplitChoices.Contains(previousSelection))
                        {
                            comboBox.SelectedItem = previousSelection;
                        }
                        else
                        {
                            comboBox.SelectedItem = string.Empty;
                        }
                    }
                }
            }
        }

        public ComponentSettings(LiveSplitState state)
        {
            _state = state;

            InitializeComponent();

            txtDevice.DataBindings.Add(nameof(TextBox.Text), this, nameof(Device), false, DataSourceUpdateMode.OnPropertyChanged);
            txtConfigFile.DataBindings.Add(nameof(TextBox.Text), this, nameof(ConfigFile), false, DataSourceUpdateMode.OnPropertyChanged);
            chkReset.DataBindings.Add(nameof(CheckBox.Checked), this, nameof(ResetSNES), false, DataSourceUpdateMode.OnPropertyChanged);
            chkStatus.DataBindings.Add(nameof(CheckBox.Checked), this, nameof(ShowStatusMessage), false, DataSourceUpdateMode.OnPropertyChanged);

            errorIcon.Image = SystemIcons.Error.ToBitmap();
        }

        public void SetSettings(XmlNode node)
        {
            var settingsElement = (XmlElement)node;
            var versionElement = settingsElement["Version"];
            int version = versionElement?.InnerText == "1.2" ? 1 : SettingsHelper.ParseInt(versionElement);
            switch (version)
            {
                case 1:
                    LoadSettings_1(settingsElement);
                    break;

                case 2:
                    LoadSettings_2(settingsElement);
                    break;

                case 3:
                    LoadSettings_3(settingsElement);
                    break;

                default:
                    Log.Error($"Tried to load Usb2SnesSplitter settings with unknown version: {version}");
                    break;
            }

            SetupSplitsPanel();
            _state.RunManuallyModified += (sender, args) => SetupSplitsPanel();
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

        internal List<string> GetSplits()
        {
            if (!ReadConfig())
            {
                return null;
            }

            if (_settingsChanged)
            {
                _settingsChanged = false;
                if (ValidateSettings())
                {
                    errorMessage.Text = null;
                    _cachedSplits = new List<string>();
                    foreach (var segment in _state.Run)
                    {
                        _cachedSplits.Add(_segmentMap[segment].AutosplitName);
                    }
                }
                else
                {
                    _cachedSplits = null;
                }
            }

            return _cachedSplits;
        }

        private bool ReadConfig()
        {
            try
            {
                var jsonStr = File.ReadAllText(ConfigFile);
                if (jsonStr != _previousConfigJson)
                {
                    _settingsChanged = true;
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    serializer.RegisterConverters(new[] { new ConfigFileJsonConverter() });
                    Config = serializer.Deserialize<Game>(jsonStr);
                    SplitChoices = _emptySplitChoices.Concat(Config.definitions.Select(split => split.name).OrderBy(name => name));
                    _previousConfigJson = jsonStr;
                }
            }
            catch (Exception e)
            {
                errorMessage.Text = "Could not read config file.\n" + e.Message;
                return false;
            }

            return true;
        }

        private void SetupSplitsPanel()
        {
            ReadConfig();

            bool segmentsChanged = false;
            if (_state.Run.Count == splitsPanel.RowCount)
            {
                int row = 0;
                foreach (var segment in _state.Run)
                {
                    var label = (Label)splitsPanel.GetControlFromPosition(0, row);
                    if (label?.Text != segment.Name)
                    {
                        segmentsChanged = true;
                        break;
                    }

                    ++row;
                }
            }
            else
            {
                segmentsChanged = true;
            }

            if (segmentsChanged)
            {
                _settingsChanged = true;
                splitsPanel.RowCount = 0;
                splitsPanel.Controls.Clear();
                splitsPanel.RowStyles.Clear();

                int rowIndex = 0;
                foreach (var segment in _state.Run)
                {
                    ++splitsPanel.RowCount;
                    splitsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    var segmentLabel = new Label
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        Margin = new Padding(3),
                        Text = $"{segment.Name}:",
                    };

                    // Add this segment to the map if it's not already there
                    if (!_segmentMap.TryGetValue(segment, out var splitSelection))
                    {
                        splitSelection = new AutosplitSelection(string.Empty);
                        _segmentMap[segment] = splitSelection;
                    }

                    if (string.IsNullOrEmpty(splitSelection.AutosplitName))
                    {
                        // This segment doesn't have an autosplit specified, so let's see if we can find a good default
                        if (_segmentNameMap.TryGetValue(segment.Name, out string autosplitName))
                        {
                            // We found a match for this segment's name in our loaded layout settings
                            splitSelection.AutosplitName = autosplitName;
                        }
                        else if (Config.definitions.Find(split => split.name == segment.Name) != null)
                        {
                            // We found an autosplit with the same name as this segment
                            splitSelection.AutosplitName = segment.Name;
                        }
                        else if (Config.alias.TryGetValue(segment.Name, out autosplitName))
                        {
                            // We found an autosplit alias with the same name as this segment
                            splitSelection.AutosplitName = autosplitName;
                        }
                    }

                    var comboBox = new ComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        DataSource = new BindingSource { DataSource = SplitChoices },
                        DropDownStyle = ComboBoxStyle.DropDownList,
                    };

                    comboBox.DataBindings.Add(nameof(ComboBox.SelectedItem), splitSelection, nameof(AutosplitSelection.AutosplitName), false, DataSourceUpdateMode.OnPropertyChanged);
                    comboBox.MouseWheel += (s, e) => ((HandledMouseEventArgs)e).Handled = true;
                    comboBox.SelectedValueChanged += (s, e) =>
                    {
                        comboBox.DataBindings[0].WriteValue();
                        _settingsChanged = true;
                    };

                    splitsPanel.Controls.Add(segmentLabel, 0, rowIndex);
                    splitsPanel.Controls.Add(comboBox, 1, rowIndex);
                    ++rowIndex;
                }
            }
        }

        private bool ValidateSettings()
        {
            if (!string.Equals(_state.Run.GameName, Config.name, StringComparison.OrdinalIgnoreCase))
            {
                errorMessage.Text = $"Game name from splits [{_state.Run.GameName}] does not match game name from config file [{Config.name}]";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Device))
            {
                errorMessage.Text = "You must specify a Device name";
                return false;
            }

            foreach (var segment in _state.Run)
            {
                if (!Config.definitions.Any(split => split.name == _segmentMap[segment].AutosplitName))
                {
                    errorMessage.Text = $"Invalid split selection for segment [{segment.Name}]";
                    return false;
                }
            }

            return true;
        }

        private int CreateSettingsNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "Version", 3) ^
            SettingsHelper.CreateSetting(document, parent, nameof(Device), Device) ^
            SettingsHelper.CreateSetting(document, parent, nameof(ConfigFile), ConfigFile) ^
            SettingsHelper.CreateSetting(document, parent, nameof(ResetSNES), ResetSNES) ^
            SettingsHelper.CreateSetting(document, parent, nameof(ShowStatusMessage), ShowStatusMessage) ^
            CreateSplitSettingsNode(document, parent);
        }

        private int CreateSplitSettingsNode(XmlDocument document, XmlElement parent)
        {
            var splitsElement = document?.CreateElement(SplitsElementName);
            parent?.AppendChild(splitsElement);

            int result = 0;
            int count = 1;
            foreach (ISegment segment in _state.Run)
            {
                if (_segmentMap.TryGetValue(segment, out var splitSelection))
                {
                    var splitElement = document?.CreateElement(SplitElementName);
                    splitsElement?.AppendChild(splitElement);

                    result ^= count *
                    (
                        SettingsHelper.CreateSetting(document, splitElement, SegmentNameElementName, segment.Name) ^
                        SettingsHelper.CreateSetting(document, splitElement, nameof(AutosplitSelection.AutosplitName), splitSelection.AutosplitName)
                    );

                    ++count;
                }
            }

            return result;
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
            LoadSettings_1(element);
            ShowStatusMessage = SettingsHelper.ParseBool(element[nameof(ShowStatusMessage)]);
        }

        private void LoadSettings_3(XmlElement settingsElement)
        {
            LoadSettings_2(settingsElement);

            _segmentNameMap.Clear();
            foreach (var splitElement in settingsElement[SplitsElementName].ChildNodes.Cast<XmlElement>())
            {
                string segmentName = SettingsHelper.ParseString(splitElement[SegmentNameElementName]);
                string autosplitName = SettingsHelper.ParseString(splitElement[nameof(AutosplitSelection.AutosplitName)]);
                _segmentNameMap[segmentName] = autosplitName;
            }
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

        private void errorMessage_TextChanged(object sender, EventArgs e)
        {
            errorPanel.Visible = !string.IsNullOrEmpty(errorMessage.Text);
        }

        private void txtDevice_TextChanged(object sender, EventArgs e)
        {
            _settingsChanged = true;
        }
    }
}
