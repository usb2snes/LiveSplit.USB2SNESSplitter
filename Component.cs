using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model;
using LiveSplit.Options;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LiveSplit.USB2SNESSplitterTests")]

namespace LiveSplit.UI.Components
{
    public class USB2SNESComponent : IComponent
    {
        enum ConfigState
        {
            NONE,
            ERROR,
            READY,
        };

        enum ProtocolState
        {
            NONE,
            CONNECTING,
            FAILED_TO_CONNECT,
            DEVICE_NOT_FOUND,
            ATTACHING,
            FAILED_TO_ATTACH,
            ATTACHED,
        }

        private const float BorderThickness = 1.5f;
        private const float TextPadding = 2f;
        private readonly TimeSpan ReadyAutoHideTime = TimeSpan.FromSeconds(3);
        // This is a bit ugly, but it's to deal with the way LiveSplit handles sizing. When first starting up, or when the layout changes,
        // LiveSplit attempts to force the window's size to whatever's specified in the layout settings. Then, once everything has stabilized,
        // further changes to the size of components do not cause the whole window to be resized to match the layout settings. If we show the
        // status message right away, that will affect the "stable" size of this component, which means that once everything is ready and the
        // message is hidden, the window will be smaller than the layout settings specify. So, we delay showing the status message for a bit
        // to give LiveSplit a chance to lock in the correct size.
        private readonly TimeSpan ShowStatusMessageDelay = TimeSpan.FromSeconds(0.5);

        public string ComponentName => "USB2SNES Auto Splitter";

        public float HorizontalWidth { get; set; }

        public float MinimumHeight => 2 * BorderThickness;

        public float VerticalHeight { get; set; }

        public float MinimumWidth => 2 * BorderThickness;

        public float PaddingTop => 1;

        public float PaddingBottom => 1;

        public float PaddingLeft => 1;

        public float PaddingRight => 1;

        public IDictionary<string, Action> ContextMenuControls => null;

        private Timer _update_timer;
        private ComponentSettings _settings;
        private LiveSplitState _state;
        private TimerModel _model;
        private List<string> _splits;
        private ConfigState _config_state;
        private ProtocolState _proto_state;
        private DateTime _first_draw_time = DateTime.MinValue;
        private Stopwatch _ready_timer;
        private string _attached_device;
        private bool _inTimer;
        private USB2SnesW.USB2SnesW _usb2snes;
        private Color _ok_color = Color.FromArgb(0, 128, 0);
        private Color _error_color = Color.FromArgb(128, 0, 0);
        private Color _connecting_color = Color.FromArgb(128, 128, 0);
        private bool _stateChanged;

        private void init(LiveSplitState state, USB2SnesW.USB2SnesW usb2snesw)
        {
            _state = state;
            _config_state = ConfigState.NONE;
            _proto_state = ProtocolState.NONE;
            _ready_timer = new Stopwatch();
            _attached_device = string.Empty;
            _settings = new ComponentSettings(_state)
            {
                Dock = DockStyle.Fill,
            };
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

        private void SetConfigState(ConfigState state)
        {
            if (_config_state != state)
            {
                _stateChanged = true;
                _config_state = state;
            }
        }

        private void SetProtocolState(ProtocolState state)
        {
            if (_proto_state != state)
            {
                _stateChanged = true;
                _proto_state = state;
            }
        }

        private void CheckConfig()
        {
            _splits = _settings.GetSplits();
            if (_splits != null)
            {
                SetConfigState(ConfigState.READY);
            }
            else
            {
                SetConfigState(ConfigState.ERROR);
            }
        }

        private void CheckConnection()
        {
            if (!_usb2snes.Connected())
            {
                if (_proto_state != ProtocolState.CONNECTING)
                {
                    SetProtocolState(ProtocolState.CONNECTING);
                    _attached_device = string.Empty;

                    Task<bool> t = _usb2snes.Connect();
                    t.ContinueWith((t1) =>
                    {
                        if (t1.Result)
                        {
                            _usb2snes.SetName("LiveSplit AutoSplitter");
                            CheckAttachment();
                        }
                        else
                        {
                            SetProtocolState(ProtocolState.FAILED_TO_CONNECT);
                        }
                    });
                }
            }
            else
            {
                CheckAttachment();
            }
        }

        private async void CheckAttachment()
        {
            if (string.IsNullOrEmpty(_attached_device) || _attached_device != _settings.Device)
            {
                if (_proto_state != ProtocolState.ATTACHING)
                {
                    SetProtocolState(ProtocolState.ATTACHING);

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

                    string device = _settings.Device;
                    if (!devices.Contains(device))
                    {
                        SetProtocolState(ProtocolState.DEVICE_NOT_FOUND);
                    }
                    else
                    {
                        _usb2snes.Attach(device);
                        var info = await _usb2snes.Info(); // Info is the only neutral way to know if we are attached to the device
                        if (string.IsNullOrEmpty(info.version))
                        {
                            SetProtocolState(ProtocolState.FAILED_TO_ATTACH);
                        }
                        else
                        {
                            _attached_device = device;
                            SetProtocolState(ProtocolState.ATTACHED);
                        }
                    }
                }
            }
            else
            {
                SetProtocolState(ProtocolState.ATTACHED);
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

        public int GetSettingsHashCode()
        {
            return _settings.GetSettingsHashCode();
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (invalidator != null && (_stateChanged || _ready_timer.IsRunning))
            {
                _stateChanged = false;
                invalidator.Invalidate(0, 0, width, height);
            }
        }

        public async Task DoSplit()
        {
            if (_settings.Config.igt != null && _settings.Config.igt.active == "1" && _usb2snes.Connected())
            {
                uint[] allAddresses = new uint[] { _settings.Config.igt.framesAddressInt, _settings.Config.igt.secondsAddressInt,
                                                   _settings.Config.igt.minutesAddressInt, _settings.Config.igt.hoursAddressInt };
                IEnumerable<uint> validAddresses = allAddresses.Where(address => address > 0);
                uint startingAddress = validAddresses.Min();
                uint igtDataSize = (validAddresses.Max() + 2) - startingAddress;
                if (0 == igtDataSize || igtDataSize > 512)
                {
                    Debug.WriteLine("DoSplit: IGT configuration invalid, skipping it");
                }
                else
                {
                    byte[] data;
                    try
                    {
                        data = await _usb2snes.GetAddress((0xF50000 + startingAddress), igtDataSize);
                    }
                    catch
                    {
                        Debug.WriteLine("DoSplit: Exception getting address");
                        _model.Split();
                        return;
                    }

                    if (data.Count() == 0)
                    {
                        Debug.WriteLine("DoSplit: Get address failed to return result");
                    }
                    else
                    {
                        Func<uint, int> readIgt = (address) =>
                                (0 == address) ? 0 : (data[address - startingAddress] + (data[(address + 1) - startingAddress] << 8));
                        int ms = (readIgt(_settings.Config.igt.framesAddressInt) * 1000) / 60;
                        int sec = readIgt(_settings.Config.igt.secondsAddressInt);
                        int min = readIgt(_settings.Config.igt.minutesAddressInt);
                        int hr = readIgt(_settings.Config.igt.hoursAddressInt);
                        var gt = new TimeSpan(0, hr, min, sec, ms);
                        _state.SetGameTime(gt);
                    }
                }
            }
            _model.Split();
        }

        private async void UpdateSplitsWrapper()
        {
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
                Log.Error($"{ComponentName} failed to update splits: {e}");
                CheckConnection();
            }
            _inTimer = false;
        }

        public async Task UpdateSplits()
        {
            if (_config_state != ConfigState.READY || _state.CurrentPhase == TimerPhase.NotRunning)
            {
                CheckConfig();
            }

            if (_state.CurrentPhase == TimerPhase.NotRunning || _proto_state != ProtocolState.ATTACHED)
            {
                CheckConnection();
            }

            if (_config_state != ConfigState.READY || _proto_state != ProtocolState.ATTACHED)
            {
                _update_timer.Interval = 1000;
            }
            else
            {
                _update_timer.Interval = 33;
                if (_state.CurrentPhase == TimerPhase.NotRunning)
                {
                    if (_settings.Config.autostart != null && _settings.Config.autostart.active == "1")
                    {
                        byte[] data;
                        try
                        {
                            data = await _usb2snes.GetAddress((0xF50000 + _settings.Config.autostart.addressInt), (uint)2);
                        }
                        catch
                        {
                            Debug.WriteLine("UpdateSplits: Exception getting address");
                            return;
                        }

                        if (data.Count() == 0)
                        {
                            Debug.WriteLine("UpdateSplits: Get address failed to return result");
                            return;
                        }

                        uint value = (uint)data[0];
                        uint word = (uint)(data[0] + (data[1] << 8));

                        switch (_settings.Config.autostart.type)
                        {
                            case "bit":
                                if ((value & _settings.Config.autostart.valueInt) != 0) { _model.Start(); }
                                break;
                            case "eq":
                                if (value == _settings.Config.autostart.valueInt) { _model.Start(); }
                                break;
                            case "gt":
                                if (value > _settings.Config.autostart.valueInt) { _model.Start(); }
                                break;
                            case "lt":
                                if (value < _settings.Config.autostart.valueInt) { _model.Start(); }
                                break;
                            case "gte":
                                if (value >= _settings.Config.autostart.valueInt) { _model.Start(); }
                                break;
                            case "lte":
                                if (value <= _settings.Config.autostart.valueInt) { _model.Start(); }
                                break;
                            case "wbit":
                                if ((word & _settings.Config.autostart.valueInt) != 0) { _model.Start(); }
                                break;
                            case "weq":
                                if (word == _settings.Config.autostart.valueInt) { _model.Start(); }
                                break;
                            case "wgt":
                                if (word > _settings.Config.autostart.valueInt) { _model.Start(); }
                                break;
                            case "wlt":
                                if (word < _settings.Config.autostart.valueInt) { _model.Start(); }
                                break;
                            case "wgte":
                                if (word >= _settings.Config.autostart.valueInt) { _model.Start(); }
                                break;
                            case "wlte":
                                if (word <= _settings.Config.autostart.valueInt) { _model.Start(); }
                                break;
                        }
                    }
                }
                else if (_state.CurrentPhase == TimerPhase.Running)
                {
                    var splitName = _splits[_state.CurrentSplitIndex];
                    var split = _settings.Config.definitions.Where(x => x.name == splitName).First();
                    var orignSplit = split;
                    if (split.next != null && split.posToCheck != 0)
                    {
                        split = split.next[split.posToCheck - 1];
                    }
                    bool ok = await doCheckSplit(split);
                    if (orignSplit.next != null && ok)
                    {
                        Debug.WriteLine("Next count :" + orignSplit.next.Count + " - Pos to check : " + orignSplit.posToCheck);
                        if (orignSplit.posToCheck < orignSplit.next.Count())
                        {
                            orignSplit.posToCheck++;
                            ok = false;
                        }
                        else
                        {
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
                        await DoSplit();
                    }
                }
            }
        }

        async Task<bool> doCheckSplit(Split split)
        {
            byte[] data;
            try
            {
                data = await _usb2snes.GetAddress((0xF50000 + split.addressInt), (uint)2);
            }
            catch
            {
                Debug.WriteLine("doCheckSplit: Exception getting address");
                CheckConnection();
                return false;
            }

            if (data.Count() == 0)
            {
                Debug.WriteLine("doCheckSplit: Get address failed to return result");
                CheckConnection();
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
            if (_first_draw_time == DateTime.MinValue)
            {
                _first_draw_time = DateTime.Now;
            }

            Color borderColor = _error_color;
            string statusMessage = string.Empty;

            if (_config_state == ConfigState.READY && _proto_state == ProtocolState.ATTACHED)
            {
                borderColor = _ok_color;

                TimeSpan elapsedTime = _ready_timer.Elapsed;
                if (elapsedTime == TimeSpan.Zero)
                {
                    _ready_timer.Start();
                }

                if (elapsedTime < ReadyAutoHideTime)
                {
                    statusMessage = $"Ready (hiding in {Math.Ceiling((ReadyAutoHideTime - elapsedTime).TotalSeconds)})";
                }
                else if (_ready_timer.IsRunning)
                {
                    _ready_timer.Stop();
                }
            }
            else
            {
                if (_ready_timer.Elapsed != TimeSpan.Zero)
                {
                    _ready_timer.Reset();
                }

                if (_config_state == ConfigState.ERROR)
                {
                    borderColor = _error_color;
                    statusMessage = $"Configuration error - check Layout Settings for {ComponentName}";
                }
                else if (_config_state == ConfigState.READY)
                {
                    switch (_proto_state)
                    {
                        case ProtocolState.CONNECTING:
                            borderColor = _connecting_color;
                            statusMessage = "Connecting to Usb2Snes";
                            break;

                        case ProtocolState.FAILED_TO_CONNECT:
                            borderColor = _error_color;
                            statusMessage = "Failed to connect to Usb2Snes";
                            break;

                        case ProtocolState.DEVICE_NOT_FOUND:
                            borderColor = _error_color;
                            statusMessage = $"Device {_settings.Device} not found. Ensure device is running a game and connected to your PC";
                            break;

                        case ProtocolState.ATTACHING:
                            borderColor = _connecting_color;
                            statusMessage = $"Attaching to {_settings.Device}";
                            break;

                        case ProtocolState.FAILED_TO_ATTACH:
                            borderColor = _error_color;
                            statusMessage = $"Failed to attach to {_settings.Device}";
                            break;
                    }
                }
            }

            float borderWidth = width - BorderThickness;
            float borderHeight = BorderThickness;

            if (_settings.ShowStatusMessage && !string.IsNullOrEmpty(statusMessage) && (DateTime.Now - _first_draw_time) > ShowStatusMessageDelay)
            {
                float textWidth = borderWidth - BorderThickness - 2 * TextPadding;

                float textHeight = g.MeasureString(statusMessage, state.LayoutSettings.TextFont, (int)textWidth).Height;
                borderHeight += textHeight + TextPadding;

                Brush textBrush = new SolidBrush(state.LayoutSettings.TextColor);
                RectangleF textRectangle = new RectangleF(TextPadding + BorderThickness, TextPadding + BorderThickness, textWidth, textHeight);
                g.DrawString(statusMessage, state.LayoutSettings.TextFont, textBrush, textRectangle);
            }

            Pen borderPen = new Pen(borderColor, BorderThickness);
            g.DrawRectangle(borderPen, BorderThickness / 2, PaddingTop + BorderThickness / 2, borderWidth, borderHeight);

            // Changing the height of this component will change the height of the whole timer form, which breaks people's ability
            // to set a size for their layout. So, if we're changing the height, save the size of the timer form to restore it next update.
            VerticalHeight = borderHeight + BorderThickness + PaddingTop + PaddingBottom;
        }
    }
}
