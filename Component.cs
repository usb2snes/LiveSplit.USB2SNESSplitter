using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model;
using LiveSplit.Options;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using USB2SnesW;
using System.Drawing;

namespace LiveSplit.UI.Components
{
    public class USB2SNESComponent : IComponent
    {
        enum MyState
        {
            NONE,
            ERROR,
            CONNECTING,
            READY,
        };
        enum ProtocolState // Only when attached we are good
        {
            NONE,
            CONNECTED,
            ATTACHED
        }
        class Split
        {
            public string name { get; set; }
            public string alias { get; set; }
            public string address { get; set; }
            public string value { get; set; }
            public string type { get; set; }
            public List<Split> more { get; set; }
            public List<Split> next { get; set; }
            public int posToCheck { get; set; } = 0;

            public uint addressint { get { return Convert.ToUInt32(address, 16); } }
            public uint valueint { get { return Convert.ToUInt32(value, 16); } }

        }

        class Category
        {
            public string name { get; set; }
            public List<string> splits { get; set; }
        }

        class Game
        {
            public string name { get; set; }
            public Autostart autostart { get; set; }
            public Dictionary<String, String> alias { get; set; }
            public List<Category> categories { get; set; }
            public List<Split> definitions { get; set; }
        }

        class Autostart
        {
            public string active { get; set; }
            public string address { get; set; }
            public string value { get; set; }
            public string type { get; set; }

            public uint addressint { get { return Convert.ToUInt32(address, 16); } }
            public uint valueint { get { return Convert.ToUInt32(value, 16); } }
        }

        public string ComponentName => "USB2SNES Auto Splitter";

        public float HorizontalWidth { get; set; }

        public float MinimumHeight => 3;

        public float VerticalHeight { get; set; }

        public float MinimumWidth => 3;

        public float PaddingTop => 1;

        public float PaddingBottom => 1;

        public float PaddingLeft => 1;

        public float PaddingRight => 1;

        public IDictionary<string, Action> ContextMenuControls => null;

        private Timer _update_timer;
        private ComponentSettings _settings;
        private LiveSplitState _state;
        private TimerModel _model;
        private Game _game;
        private List<string> _splits;
        private MyState _mystate;
        private ProtocolState _proto_state;
        private bool _inTimer;
        private bool _error;
        private bool _ready_to_start;
        private bool _valid_config;
        private bool _config_checked;
        private USB2SnesW.USB2SnesW _usb2snes;
        private Color _ok_color = Color.FromArgb(0, 128, 0);
        private Color _error_color = Color.FromArgb(128, 0, 0);
        private Color _connecting_color = Color.FromArgb(128, 128,0);
        bool _stateChanged;

        public USB2SNESComponent(LiveSplitState state)
        {
            _state = state;
            _mystate = MyState.NONE;
            _proto_state = ProtocolState.NONE;
            _settings = new ComponentSettings();
            _model = new TimerModel() { CurrentState = _state };
            _state.RegisterTimerModel(_model);
            _stateChanged = false;
            _splits = null;
            _inTimer = false;
            _error = false;
            _ready_to_start = false;
            _config_checked = false;
            _valid_config = false;

            _update_timer = new Timer() { Interval = 1000 };
            _update_timer.Tick += (sender, args) => UpdateSplits();
            _update_timer.Enabled = true;

            _state.OnReset += _state_OnReset;
            _state.OnStart += _state_OnStart;
            _usb2snes = new USB2SnesW.USB2SnesW();
            HorizontalWidth = 3;
            VerticalHeight = 3;
        }

        private void ShowMessage(String msg)
        {
            MessageBox.Show(msg, "USB2Snes AutoSplitter");
        }
        private void SetState(MyState state)
        {
            Console.WriteLine("Setting state to " + state);
            _stateChanged = true;
            _mystate = state;
        }

        private async void wsAttach(ProtocolState prevState)
        {
            List<String> devices = await _usb2snes.GetDevices();
            if (!devices.Contains(_settings.Device))
            {
                if (prevState == ProtocolState.NONE)
                    ShowMessage("Could not find the device : " + _settings.Device + " . Check your configuration or activate your device.");
                return;
            }
            _usb2snes.Attach(_settings.Device);
            var info = await _usb2snes.Info(); // Info is the only neutral way to know if we are attached to the device
            if (info.version == "")
            {
                SetState(MyState.ERROR);
            }
            else
            {
                SetState(MyState.READY);
                _proto_state = ProtocolState.ATTACHED;
            }
        }

        private void connect()
        {
            ProtocolState prevState = _proto_state;
            if (!_usb2snes.Connected() || _proto_state != ProtocolState.CONNECTED)
            {
                SetState(MyState.CONNECTING);
                Task t = _usb2snes.Connect();
                t.ContinueWith((t1) =>
                {
                    if (!_usb2snes.Connected())
                    {
                        SetState(MyState.NONE);
                        _proto_state = ProtocolState.NONE;
                        return;
                    }
                    _usb2snes.SetName("LiveSplit AutoSpliter");
                    _proto_state = ProtocolState.CONNECTED;
                    wsAttach(prevState);
                });
            } else
            {
                if (_usb2snes.Connected())
                    wsAttach(prevState);
            }
        }

        private bool readConfig()
        {
            try
            {
                var jsonStr = File.ReadAllText(_settings.ConfigFile);
                _game = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Game>(jsonStr);
            }
            catch (Exception e)
            {
                ShowMessage("Could not open split config file, check config file settings." + e.Message);
                return false;
            }
            if (!this.checkSplitsSetting())
            {
                ShowMessage("The split config file has missing definitions.");
                return false;
            }

            return true;
        }

        private bool checkSplitsSetting()
        {
            bool r = true;
            foreach (var c in _game.categories)
            {
                foreach (var s in c.splits)
                {
                    var d = _game.definitions.Where(x => x.name == s).FirstOrDefault();
                    if (d == null)
                    {
                        ShowMessage(String.Format("Split definition missing: {0} for category {1}", s, c.name));
                        r = false;
                    }
                }
            }
            if (_game.alias != null)
            {
                foreach (var a in _game.alias)
                {
                    var d = _game.definitions.Where(x => x.name == a.Value).FirstOrDefault();
                    if (d == null)
                    {
                        ShowMessage(String.Format("Alias definition <{0}> does not point to a split name in a category definition : {1}", a.Key, a.Value));
                        r = false;
                    }
                }
            }
            return r;
        }

        private bool checkRunnableSetting()
        {
            bool toret = true;
            _splits = new List<string>(_game.categories.Where(c => c.name.ToLower() == _state.Run.CategoryName.ToLower()).First()?.splits);

            if (_splits.Count == 0)
            {
                ShowMessage("There are no splits for the current category in the split config file, check that the run category is correctly set and exists in the config file.");
                return false;
            }
            if (_state.Run.Count() >_splits.Count())
            {
                ShowMessage(String.Format("There is more segment in your splits configuration <{0}> than the Autosplitter setting file <{1}>", _splits.Count(), _state.Run.Count()));
                _error = true;
                return false;
            }
            foreach (var seg in _state.Run)
            {
                if (!_splits.Contains(seg.Name, StringComparer.OrdinalIgnoreCase))
                {
                    // Searching into Alias
                    if (!_game.alias.ContainsKey(seg.Name))
                    {
                        ShowMessage(String.Format("Your segment name <{0}> does not exist in the setting file. Neither as a split name or an alias.", seg.Name));
                        toret = false;
                    }
                }
            }
            return toret;
        }

        // Let's build the split list based on the user segment list and not the category definition
        private void SetSplitList()
        {
            _splits.Clear();
            var catSplits = _game.categories.Where(c => c.name.ToLower() == _state.Run.CategoryName.ToLower()).First().splits;
            foreach (var seg in _state.Run)
            {
                if (catSplits.Contains(seg.Name))
                    _splits.Add(seg.Name);
                else
                    _splits.Add(_game.alias[seg.Name]);
            }
        }

        private void _state_OnStart(object sender, EventArgs e)
        {
            Console.WriteLine("On START?");
            return;
            /*
            if(_game == null)
            {
                if(!this.readConfig())
                {
                    _model.Reset();
                    return;
                }
            }

            _error = false;

            
            if (!_usb2snes.Connected())
            {
                if (!this.connect())
                {
                    _model.Reset();
                    return;
                }
            }*/
        }

        private void _state_OnReset(object sender, TimerPhase value)
        {
            if (_usb2snes.Connected())
            {
                if(_settings.ResetSNES)
                {
                    _usb2snes.Reset();
                }
            }
        }

        public void Dispose()
        {
            _update_timer?.Dispose();
            if (_usb2snes.Connected())
            {
                _usb2snes.Disconnect();
            }
            _state.OnStart -= _state_OnStart;
            _state.OnReset -= _state_OnReset;
            //_state.OnUndoSplit -= OnUndoSplit;
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return _settings;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            return _settings.GetSettings(document);
        }

        public void SetSettings(XmlNode settings)
        {
            _settings.SetSettings(settings);
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height,
            LayoutMode mode)
        {
            if (invalidator != null && _stateChanged)
            {
                _stateChanged = false;
                invalidator.Invalidate(0, 0, width, height);
            }
        }

        public async void DoSplit()
        {
            if (_game.name == "Super Metroid" && _usb2snes.Connected())
            {
                var data = new byte[512];
                data = await _usb2snes.GetAddress((uint)(0xF509DA), (uint)512);
                int ms = (data[0] + (data[1] << 8)) * (1000 / 60);
                int sec = data[2] + (data[3] << 8);
                int min = data[4] + (data[5] << 8);
                int hr = data[6] + (data[7] << 8);
                var gt = new TimeSpan(0, hr, min, sec, ms);
                _state.SetGameTime(gt);
                _model.Split();
            }
            else
            {
                _model.Split();
            }
        }

        private bool checkSplit(Split split, uint value, uint word)
        {
            bool ret = false;
            switch (split.type)
            {
                case "bit":
                    if ((value & split.valueint) != 0) { ret = true; }
                    break;
                case "eq":
                    if (value == split.valueint) { ret = true; }
                    break;
                case "gt":
                    if (value > split.valueint) { ret = true; }
                    break;
                case "lt":
                    if (value < split.valueint) { ret = true; }
                    break;
                case "gte":
                    if (value >= split.valueint) { ret = true; }
                    break;
                case "lte":
                    if (value <= split.valueint) { ret = true; }
                    break;
                case "wbit":
                    if ((word & split.valueint) != 0) { ret = true; }
                    break;
                case "weq":
                    if (word == split.valueint) { ret = true; }
                    break;
                case "wgt":
                    if (word > split.valueint) { ret = true; }
                    break;
                case "wlt":
                    if (word < split.valueint) { ret = true; }
                    break;
                case "wgte":
                    if (word >= split.valueint) { ret = true; }
                    break;
                case "wlte":
                    if (word <= split.valueint) { ret = true; }
                    break;
            }
            return ret;
        }

        private bool isConfigReady()
        {
            if (!_config_checked)
            {
                if (this.readConfig())
                {
                    if (_config_checked == false && checkRunnableSetting())
                    {
                        _valid_config = true;
                        SetSplitList();
                    }
                }
                _config_checked = true;
            }
            if (!_valid_config)
                return false;
            return true;
        }

        private bool isConnectionReady()
        {
            Console.WriteLine("Checking connection");
            if (_usb2snes.Connected() && _proto_state == ProtocolState.ATTACHED)
                return true;
            if (!_usb2snes.Connected())
            {
                SetState(MyState.NONE);
                _proto_state = ProtocolState.NONE;
            }
            this.connect();
            return false;
        }

        public async void UpdateSplits()
        {
            if (_inTimer == true)
                return;

            _inTimer = true;
            if (_state.CurrentPhase == TimerPhase.NotRunning)
            {
                if (!isConfigReady())
                {
                    _inTimer = false;
                    return;
                }
                if (!isConnectionReady())
                {
                    _update_timer.Interval = 1000;
                    _inTimer = false;
                    return;
                } else  {
                        _update_timer.Interval = 33;
                        _ready_to_start = true;
                }
                if (_game != null && _game.autostart.active == "1")
                {
                    if (_proto_state == ProtocolState.ATTACHED)
                    {
                        var data = new byte[64];
                        try
                        {
                            data = await _usb2snes.GetAddress((0xF50000 + _game.autostart.addressint), (uint)64);
                        }
                        catch
                        {
                            _inTimer = false;
                            return;
                        }
                        if (data.Count() == 0)
                        {
                            Console.WriteLine("Get address failed to return result");
                            _inTimer = false;
                            return;
                        }
                        uint value = (uint)data[0];
                        uint word = (uint)(data[0] + (data[1] << 8));

                        switch (_game.autostart.type)
                        {
                            case "bit":
                                if ((value & _game.autostart.valueint) != 0) { _model.Start(); }
                                break;
                            case "eq":
                                if (value == _game.autostart.valueint) { _model.Start(); }
                                break;
                            case "gt":
                                if (value > _game.autostart.valueint) { _model.Start(); }
                                break;
                            case "lt":
                                if (value < _game.autostart.valueint) { _model.Start(); }
                                break;
                            case "gte":
                                if (value >= _game.autostart.valueint) { _model.Start(); }
                                break;
                            case "lte":
                                if (value <= _game.autostart.valueint) { _model.Start(); }
                                break;
                            case "wbit":
                                if ((word & _game.autostart.valueint) != 0) { _model.Start(); }
                                break;
                            case "weq":
                                if (word == _game.autostart.valueint) { _model.Start(); }
                                break;
                            case "wgt":
                                if (word > _game.autostart.valueint) { _model.Start(); }
                                break;
                            case "wlt":
                                if (word < _game.autostart.valueint) { _model.Start(); }
                                break;
                            case "wgte":
                                if (word >= _game.autostart.valueint) { _model.Start(); }
                                break;
                            case "wlte":
                                if (word <= _game.autostart.valueint) { _model.Start(); }
                                break;
                        }
                    }
                }
            }
            else if (_state.CurrentPhase == TimerPhase.Running)
            {
                if (_splits != null)
                {
                    if (_proto_state == ProtocolState.ATTACHED)
                    {
                        var splitName = _splits[_state.CurrentSplitIndex];
                        var split = _game.definitions.Where(x => x.name == splitName).First();
                        var orignSplit = split;
                        if (split.next != null && split.posToCheck != 0)
                        {
                            split = split.next[split.posToCheck - 1];
                        }
                        var data = new byte[64];
                        try
                        {
                            data = await _usb2snes.GetAddress((0xF50000 + split.addressint), (uint)64);
                        }
                        catch
                        {
                            _inTimer = false;
                            return;
                        }
                        if (data.Count() == 0)
                        {
                            Console.WriteLine("Get address failed to return result");
                            _inTimer = false;
                            return;
                        }
                        uint value = (uint)data[0];
                        uint word = (uint)(data[0] + (data[1] << 8));
                        bool ok = checkSplit(split, value, word);
                        if (split.next != null && ok)
                        {
                            if (split.posToCheck != split.next.Count())
                            {
                                orignSplit.posToCheck++;
                                ok = false;
                            } else
                            {
                                orignSplit.posToCheck = 0;
                            }
                        }
                        if (split.more != null)
                        {
                            foreach (var moreSplit in split.more)
                            {
                                try
                                {
                                    data = await _usb2snes.GetAddress((0xF50000 + split.addressint), (uint)64);
                                }
                                catch
                                {
                                    _inTimer = false;
                                    return;
                                }
                                if (data.Count() == 0)
                                {
                                    Console.WriteLine("Get address failed to return result");
                                    _inTimer = false;
                                    return;
                                }
                                value = (uint)data[0];
                                word = (uint)(data[0] + (data[1] << 8));

                                ok = ok && checkSplit(moreSplit, value, word);
                            }
                        }

                        if (ok)
                        {
                            DoSplit();
                        }
                    }
                }
            }
            _inTimer = false;
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            VerticalHeight = height;
            HorizontalWidth = 3;
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            VerticalHeight = 3 + PaddingTop + PaddingBottom;
            HorizontalWidth = width;
            Color col;
            Console.WriteLine(_mystate);
            switch (_mystate)
            {
                case MyState.READY: col = _ok_color; break;
                case MyState.CONNECTING: col = _connecting_color; break;
                default: col = _error_color; break;
            }
            Brush b = new SolidBrush(col);
            g.FillRectangle(b, 0, 0, width, 3);
        }
    }
}
