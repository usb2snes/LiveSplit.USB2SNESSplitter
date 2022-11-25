using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;

using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics;
using USB2SnesW;
using System.Timers;

namespace LiveSplit.UI.Components
{
    public partial class MainUI: Form
    {
        enum Usb2SnesState
        {
            NoState,
            TryingToConnect,
            Connected,
            GettingDevices,
            NoDevice,
            TryingToAttach,
            Attached,
            Ready
        };
        private Game game;
        private Split currentSplit;
        private String fileName = null;
        private bool hasNext = false;
        private bool hasMore = false;
        private string attachedDevice;
        private USB2SnesW.USB2SnesW usb2snes;
        private Usb2SnesState connectionState;
        private System.Timers.Timer usb2snesCoTimer;
        public MainUI()
        {
            InitializeComponent();
            usb2snes = new USB2SnesW.USB2SnesW();
            connectionState = Usb2SnesState.NoState;
            usb2snesCoTimer = new System.Timers.Timer(1000);
            usb2snesCoTimer.Elapsed += onTimerElapsed;
        }
        private void onTimerElapsed(Object source, ElapsedEventArgs e)
        {
            if (connectionState != Usb2SnesState.Ready)
                tryToConnect();
        }
        async private void tryToConnect()
        {
            connectionState = Usb2SnesState.TryingToConnect;
            if (connectionState == Usb2SnesState.NoState)
            {
                bool co = await usb2snes.Connect();
                if (co)
                {
                    connectionState = Usb2SnesState.Connected;
                    usb2snes.SetName("LiveSplit Definition Helper");
                }
                else
                {
                    connectionState = Usb2SnesState.NoState;
                    usb2snesLabel.Text = "Failed to connect to Usb2Snes";
                    return;
                }
            }
            if (connectionState == Usb2SnesState.Connected)
            {
                List<String> devices;
                usb2snesLabel.Text = "Trying to get devices";
                try
                {
                    connectionState = Usb2SnesState.GettingDevices;
                    devices = await usb2snes.GetDevices();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception getting devices: " + e);
                    devices = new List<String>();
                }
                if (devices.Count == 0)
                {
                    usb2snesLabel.Text = "No device found";
                    connectionState = Usb2SnesState.Connected;
                    return;
                }
                usb2snes.Attach(devices[0]);
                connectionState = Usb2SnesState.TryingToAttach;
                var info = await usb2snes.Info(); // Info is the only neutral way to know if we are attached to the device
                if (string.IsNullOrEmpty(info.version))
                {
                    connectionState = Usb2SnesState.Connected;
                }
                else
                {
                    connectionState = Usb2SnesState.Attached;
                    attachedDevice = devices[0];
                    //SetProtocolState(ProtocolState.ATTACHED);
                    connectionState = Usb2SnesState.Ready;
                    usb2snesLabel.Text = "Usb2Snes connection ready";
                }
            }
        }

        private void OpenGame_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Please Choose a json to open";
            openFileDialog.Filter = "Json File | *.json";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string configJson;
                try
                {
                    fileName = openFileDialog.FileName;
                    configJson = File.ReadAllText(openFileDialog.FileName);
                }
                catch (Exception exp)
                {
                    return;
                }
                game = Game.FromJSON(configJson);
                clearUi();
                TitleLabel.Text = game.name;
                listSplits.Items.Add("Autostart");
                foreach (var def in game.definitions)
                {
                    listSplits.Items.Add(def.name);
                }
            }
        }
        private void clearUi()
        {
            listSplits.Clear();
            addressTextBox.Clear();
            subSplitAddress.Clear();
            splitNameTextBox.Clear();
            subSplitTypeComboBox.Text = "";
            typeComboBox.Text = "";
        }

        private void listSplits_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listSplits.SelectedItems.Count == 0)
                return;
            var itemName = listSplits.SelectedItems[0];
            if (itemName.Text == "Autostart")
            {
                currentSplit = game.autostart;
                return;
            }
            foreach (var def in game.definitions)
            {
                if (itemName.Text == def.name)
                {
                    currentSplit = def;
                    updateSplitInfos();
                    break;
                }
            }

        }
        private void updateSplitInfos()
        {
            splitNameTextBox.Text = currentSplit.name;
            addressTextBox.Text = currentSplit.address;
            typeComboBox.Text = currentSplit.type;
            subSplitView.Items.Clear();
            subSplitAddress.Text = "";
            subSplitTypeComboBox.Text = "";
            hasMore = false;
            hasNext = false;
            addMoreButton.Enabled = true;
            addNextButton.Enabled = true;
            if (currentSplit.next != null)
            {
                hasNext = true;
                addMoreButton.Enabled = false;
                subSplitLabel.Text = "Has Next";
                foreach (var sp in currentSplit.next)
                {
                    subSplitView.Items.Add(sp.address);
                }
            }
            if (currentSplit.more != null)
            {
                hasMore = true;
                addNextButton.Enabled = false;
                subSplitLabel.Text = "Has More";
                foreach (var sp in currentSplit.more)
                {
                    subSplitView.Items.Add(sp.address);
                }
            }
        }

        private void splitNameTextBox_TextChanged(object sender, EventArgs e)
        {
            currentSplit.name = splitNameTextBox.Text;
            Console.WriteLine("Changing name");
            if (listSplits.SelectedItems.Count > 0)
            {
                Console.WriteLine("Changing text of selected item");
                listSplits.SelectedItems[0].Text = currentSplit.name;
            }
        }

        private void subSplitView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subSplitView.SelectedItems.Count == 0)
                return;
            Split subSplit = null;
            if (hasNext)
                subSplit = currentSplit.next[subSplitView.SelectedItems[0].Index];
            if (hasMore)
                subSplit = currentSplit.more[subSplitView.SelectedItems[0].Index];
            subSplitAddress.Text = subSplit.address;
            subSplitTypeComboBox.Text = subSplit.type;
        }

        private void NewGame_Click(object sender, EventArgs e)
        {
            NewGame diag = new NewGame();
            var result = diag.ShowDialog();
            fileName = null;
            if (result == DialogResult.OK)
            {
                game = new Game();
                game.autostart = new Autostart();
                game.autostart.active = "0";
                clearUi();
                listSplits.Items.Add("Autostart");
                game.name = diag.gameName;
                TitleLabel.Text = game.name;
                game.definitions = new List<Split>();
            }
            
        }

        private void addSplitButton_Click(object sender, EventArgs e)
        {
            Split newSplit = new Split();
            newSplit.name = "New Definition";
            newSplit.address = "0x0";
            newSplit.type = "eq";
            game.definitions.Add(newSplit);
            listSplits.Items.Add(newSplit.name);
            listSplits.Items[listSplits.Items.Count - 1].Selected = true;
            listSplits.Select();
            currentSplit = newSplit;
            //updateSplitInfos();
        }

        private void SaveGame_Click(object sender, EventArgs e)
        {
            if (fileName == null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                var result = saveFileDialog.ShowDialog();
                if (result == DialogResult.OK)
                    fileName = saveFileDialog.FileName;
            }
            String json = game.toJson();
            File.WriteAllText(fileName, json);
        }
    }
}
