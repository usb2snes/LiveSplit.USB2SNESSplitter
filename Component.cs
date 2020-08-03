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
using System.Collections;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LiveSplit.USB2SNESSplitterTests")]

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
        private USB2SnesW.USB2SnesW _usb2snes;
        private Color _ok_color = Color.FromArgb(0, 128, 0);
        private Color _error_color = Color.FromArgb(128, 0, 0);
        private Color _connecting_color = Color.FromArgb(128, 128, 0);
        bool _stateChanged;

        private void init(LiveSplitState state, USB2SnesW.USB2SnesW usb2snesw)
        {
            _state = state;
            _mystate = MyState.NONE;
            _proto_state = ProtocolState.NONE;
            _settings = new ComponentSettings();
            _model = new TimerModel() { CurrentState = _state };
            _state.RegisterTimerModel(_model);
            _stateChanged = false;
            _splits = new List<string>();
            _inTimer = false;

            _update_timer = new Timer() { Interval = 1000 };
            _update_timer.Tick += (sender, args) => UpdateSplitsWrapper();
            _update_timer.Enabled = true;

            _state.OnReset += _state_OnReset;
            _state.OnStart += _state_OnStart;
            HorizontalWidth = 3;
            VerticalHeight = 3;
            _usb2snes = usb2snesw;
        }

        public USB2SNESComponent(LiveSplitState state)
        {
            init(state, new USB2SnesW.USB2SnesW());
        }

        internal USB2SNESComponent(LiveSplitState state, USB2SnesW.USB2SnesW usb2snesw)
        {
            init(state, usb2snesw);

        }

        private void ShowMessage(String msg)
        {
            MessageBox.Show(msg, "USB2Snes AutoSplitter");
        }
        private void SetState(MyState state)
        {
            if (_mystate == state)
            {
                return;
            }
            Debug.WriteLine("Setting state to " + state);
            _stateChanged = true;
            _mystate = state;
        }

        private async void wsAttach(ProtocolState prevState)
        {
            List<String> devices;
            try
            {
                devices = await _usb2snes.GetDevices();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception getting devices: " + e);
                devices = new List<String>();
            }

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
            } else {
                _proto_state = ProtocolState.ATTACHED;
            }
        }

        private void connect()
        {
            ProtocolState prevState = _proto_state;
            var connected = _usb2snes.Connected();
            if (_proto_state != ProtocolState.CONNECTED || !connected)
            {
                SetState(MyState.CONNECTING);
                Task<bool> t = _usb2snes.Connect();
                t.ContinueWith((t1) =>
                {
                    if (!t1.Result)
                    {
                        SetState(MyState.NONE);
                        _proto_state = ProtocolState.NONE;
                        return;
                    }
                    _usb2snes.SetName("LiveSplit AutoSplitter");
                    _proto_state = ProtocolState.CONNECTED;
                    wsAttach(prevState);
                });
            } else {
                if (connected)
                    wsAttach(prevState);
            }
        }

        private bool readConfig()
        {
            try
            {
                var jsonStr = File.ReadAllText(_settings.ConfigFile);
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new ConfigFileJsonConverter() });
                _game = serializer.Deserialize<Game>(jsonStr);
            }
            catch (Exception e)
            {
                _settings.SetError("Could not open split config file, check config file settings.\n" + e.Message);
                return false;
            }

            return checkSplitsSetting();
        }

        private bool checkSplitsSetting()
        {
            var errorMessages = new List<string>();
            foreach (var c in _game.categories)
            {
                foreach (var s in c.splits)
                {
                    var d = _game.definitions.Where(x => x.name == s).FirstOrDefault();
                    if (d == null)
                    {
                        errorMessages.Add($"Split definition missing: {s} for category {c.name}");
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
                        errorMessages.Add($"Alias definition <{a.Key}> does not point to a split name in a category definition : {a.Value}");
                    }
                }
            }

            if (errorMessages.Count > 0)
            {
                _settings.SetError(string.Join("\n", errorMessages));
                return false;
            }

            return true;
        }

        private bool checkRunnableSetting()
        {
            if (!string.Equals(_state.Run.GameName, _game.name, StringComparison.OrdinalIgnoreCase))
            {
                _settings.SetError($"Game name from splits [{_state.Run.GameName}] does not match game name from config file [{_game.name}]");
                return false;
            }

            Category category = _game.categories.Where(c => c.name.ToLower() == _state.Run.CategoryName.ToLower()).FirstOrDefault();
            if (category == null)
            {
                _settings.SetError($"Category name from splits [{_state.Run.CategoryName}] not found in config file.");
                return false;
            }

            var unrecognizedSegmentNames = new List<string>();
            foreach (var seg in _state.Run)
            {
                if (!category.splits.Contains(seg.Name, StringComparer.OrdinalIgnoreCase))
                {
                    // Searching into Alias
                    if (!_game.alias.ContainsKey(seg.Name))
                    {
                        unrecognizedSegmentNames.Add(seg.Name);
                    }
                }
            }

            if (unrecognizedSegmentNames.Count > 0)
            {
                string segmentList = string.Join("\n", unrecognizedSegmentNames.Select(name => $"[{name}]"));
                _settings.SetError($"Segment names in splits could not be found in config file:\n{segmentList}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_settings.Device))
            {
                _settings.SetError("You must specify a Device name");
                return false;
            }

            return true;
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
        }

        private void _state_OnReset(object sender, TimerPhase value)
        {
            if (_usb2snes.Connected())
            {
                if (_settings.ResetSNES)
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
                var data = await _usb2snes.GetAddress((uint)(0xF509DA), (uint)512);
                int ms = (data[0] + (data[1] << 8)) * (1000 / 60);
                int sec = data[2] + (data[3] << 8);
                int min = data[4] + (data[5] << 8);
                int hr = data[6] + (data[7] << 8);
                var gt = new TimeSpan(0, hr, min, sec, ms);
                _state.SetGameTime(gt);
                _model.Split();
            } else {
                _model.Split();
            }
        }

        private bool isConfigReady()
        {
            if (this.readConfig())
            {
                if (checkRunnableSetting())
                {
                    _settings.SetError(null);
                    SetSplitList();
                    return true;
                }
            }

            return false;
        }

        private bool isConnectionReady()
        {
            Debug.WriteLine("Checking connection");
            if (_proto_state == ProtocolState.ATTACHED)
                return true;

            // this method actually does a BLOCKING request-response cycle (!!)
            bool connected = _usb2snes.Connected();
            if (!connected)
            {
                SetState(MyState.NONE);
                _proto_state = ProtocolState.NONE;
            }
            this.connect();
            return false;
        }

        private async void UpdateSplitsWrapper()
        {

            Debug.WriteLine("Timer tick " + DateTime.Now);
            // "_inTimer" is a very questionable attempt at locking, but it's probably fine here.
            if (_inTimer)
            {
                Debug.WriteLine("In timer already! !!!");
                return;
            }
            _inTimer = true;
            try
            {
                await UpdateSplits();
            }
            catch (Exception e)
            {
                ShowMessage("Something bad happened: " + e.ToString());
                connect();
            }
            _inTimer = false;
        }

        public async
        Task
UpdateSplits()
        {
            if (_state.CurrentPhase == TimerPhase.NotRunning)
            {
                if (!isConfigReady())
                {
                    SetState(MyState.ERROR);
                    return;
                }
                if (!isConnectionReady())
                {
                    _update_timer.Interval = 1000;
                    return;
                } else {
                    _update_timer.Interval = 33;
                }

                SetState(MyState.READY);

                if (_game != null && _game.autostart.active == "1")
                {
                    if (_proto_state == ProtocolState.ATTACHED)
                    {
                        byte[] data;
                        try
                        {
                            data = await _usb2snes.GetAddress((0xF50000 + _game.autostart.addressint), (uint)2);
                        }
                        catch
                        {
                            return;
                        }
                        if (data.Count() == 0)
                        {
                            Debug.WriteLine("Get address failed to return result");
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
            } else if (_state.CurrentPhase == TimerPhase.Running) {
                if (_mystate == MyState.READY)
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
                        bool ok = await (doCheckSplit(split));
                        if (orignSplit.next != null && ok)
                        {
                            Debug.WriteLine("Next count :" + orignSplit.next.Count + " - Pos to check : " + orignSplit.posToCheck);
                            if (orignSplit.posToCheck < orignSplit.next.Count())
                            {
                                orignSplit.posToCheck++;
                                ok = false;
                            } else {
                                orignSplit.posToCheck = 0;
                            }
                        }
                        if (split.more != null)
                        {
                            foreach (var moreSplit in split.more)
                            {
                                if (!ok)
                                {
                                    break;
                                }
                                ok = ok && await doCheckSplit(moreSplit);
                            }
                        }

                        if (ok)
                        {
                            DoSplit();
                        }
                    } else {
                        connect();
                    }
                }
            }
        }

        async Task<bool> doCheckSplit(Split split)
        {
            byte[] data;
            try
            {
                data = await _usb2snes.GetAddress((0xF50000 + split.addressint), (uint)2);
            }
            catch
            {
                return false;
            }
            if (data.Count() == 0)
            {
                Console.WriteLine("Get address failed to return result");
                return false;
            }
            uint value = (uint)data[0];
            uint word = (uint)(data[0] + (data[1] << 8));
            Debug.WriteLine("Address checked : " + split.address + " - value : " + value);
            return split.check(value, word);
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
