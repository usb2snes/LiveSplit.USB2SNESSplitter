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
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
        private int subSplitIndex = 0;
        private String fileName = null;
        private bool hasNext = false;
        private bool hasMore = false;
        private string attachedDevice;
        private USB2SnesW.USB2SnesW usb2snes;
        private Usb2SnesState connectionState;
        private DispatcherTimer usb2snesCoTimer;
        private SplitChecker splitChecker;
        public MainUI()
        {
            InitializeComponent();
            usb2snes = new USB2SnesW.USB2SnesW();
            connectionState = Usb2SnesState.NoState;
            usb2snesCoTimer = new DispatcherTimer();
            usb2snesCoTimer.Interval = new TimeSpan(0, 0, 3);
            usb2snesCoTimer.Start();
            usb2snesCoTimer.Tick += onTimerElapsed;
            splitChecker = new SplitChecker(this, usb2snes);
            splitChecker.onSplitOk = splitCheckIsGood;
            splitChecker.onSplitNok = splitCheckIsBad;
            splitChecker.onCheckedSplit = currentlyChecking;
            splitChecker.onUsb2snesError = usb2snesCommandFailed;
        }
        private void splitCheckIsGood()
        {
            splitOkButton.BackColor = Color.Green;
            checkButton.Text = "Check";
        }
        private void splitCheckIsBad()
        {
            splitOkButton.BackColor = Color.Orange;
        }
        private void currentlyChecking(Split split, uint word)
        {
            checkStatusLabel.Text = String.Format("Value of address {0:X} : {1:X}", split.addressInt, word);
        }
        private void onTimerElapsed(Object source, EventArgs e)
        {
            //Console.WriteLine("Usb2Snes connection timer timout");
            if (connectionState != Usb2SnesState.Ready)
                tryToConnect();
        }
        private void usb2snesCommandFailed()
        {
            connectionState = Usb2SnesState.NoState;
        }
        async private void tryToConnect()
        {
            //Console.WriteLine("Trying to Connect");
            if (connectionState == Usb2SnesState.NoState)
            {
                usb2snesLabel.Text = "Trying to connect";
                connectionState = Usb2SnesState.TryingToConnect;
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
                    usb2snesCommandFailed();
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
                    checkButton.Enabled = true;
                }
            }
        }

        public void changeSubSplitStatus(int index, Color col)
        {
            subSplitView.Rows[index].Cells["subSplitStatus"].Style.BackColor = col;
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
                if (game.definitions != null)
                {
                    foreach (var def in game.definitions)
                    {
                        listSplits.Items.Add(def.name);
                    }
                }
                listSplits.Items[0].Selected = true;
                //currentSplit = game.autostart.GetSplit();
                //updateSplitInfos();
            }
        }
        private void clearUi()
        {
            listSplits.Clear();
            addressTextBox.Clear();
            subSplitAddress.Clear();
            valueTextBox.Clear();
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
                updateSplitInfos();
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
            Console.WriteLine("Update split info");
            splitNameTextBox.Text = currentSplit.name;
            addressTextBox.Text = currentSplit.address;
            typeComboBox.Text = currentSplit.type;
            valueTextBox.Text = currentSplit.value;
            subSplitView.Rows.Clear();
            hasMore = false;
            hasNext = false;
            if (currentSplit.next == null && currentSplit.more == null)
            {
                addMoreButton.Enabled = true;
                addNextButton.Enabled = true;
            }
            else
            {
                addMoreButton.Enabled = false;
                addNextButton.Enabled = false;
            }

            if (currentSplit.next != null)
            {
                hasNext = true;
                addMoreButton.Enabled = false;
                addNextButton.Enabled = true;
                subSplitLabel.Text = "Has Next";
                int cpt = 1;
                foreach (var sp in currentSplit.next)
                {
                    if (sp.name != null)
                        subSplitView.Rows.Add(sp.name);
                    else
                        subSplitView.Rows.Add(String.Format("Next {0}", cpt));
                    cpt++;
                }
            }
            if (currentSplit.more != null)
            {
                hasMore = true;
                addNextButton.Enabled = false;
                addMoreButton.Enabled = true;
                subSplitLabel.Text = "Has More";
                int cpt = 1;
                foreach (var sp in currentSplit.more)
                {
                    if (sp.name != null)
                        subSplitView.Rows.Add(sp.name);
                    else
                        subSplitView.Rows.Add(String.Format("Next {0}", cpt));
                    cpt++;
                }
            }
            subSplitAddress.Text = "";
            subSplitTypeComboBox.Text = "";
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
        private void addressTextBox_TextChanged(object sender, EventArgs e)
        {
            currentSplit.address = addressTextBox.Text;
        }
        private void typeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentSplit.type = typeComboBox.Text;
        }
        private void valueTextBox_TextChanged(object sender, EventArgs e)
        {
            currentSplit.value = valueTextBox.Text;
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
                game.autostart.name = "Autostart";
                clearUi();
                listSplits.Items.Add("Autostart");
                currentSplit = game.autostart;
                game.name = diag.gameName;
                TitleLabel.Text = game.name;
                game.definitions = new List<Split>();
                updateSplitInfos();
            }
            
        }

        private void addSplitButton_Click(object sender, EventArgs e)
        {
            Split newSplit = new Split();
            newSplit.name = "New Definition";
            newSplit.address = "0x0";
            newSplit.type = "eq";
            newSplit.value = "0x0";
            if (game.definitions == null)
                game.definitions = new List<Split>();
            game.definitions.Add(newSplit);
            listSplits.Items.Add(newSplit.name);
            listSplits.SelectedItems.Clear();
            listSplits.Items[listSplits.Items.Count - 1].Selected = true;
            currentSplit = newSplit;
            updateSplitInfos();
            listSplits.Select();
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
        private void initCheck()
        {
            if (hasNext || hasMore)
            {
                foreach (DataGridViewRow row in subSplitView.Rows)
                {
                    row.Cells["subSplitStatus"].Style.BackColor = Color.Gray;
                }
                currentSplit.posToCheck = 0;
            }
        }
        private void checkButton_Click(object sender, EventArgs e)
        {
            if (splitChecker.isActivated)
            {
                checkButton.Text = "Check Split";
                splitOkButton.BackColor = Color.Gray;
                splitChecker.StopCheck();
            }
            else
            {
                initCheck();
                checkButton.Text = "Stop Checking";
                splitOkButton.BackColor = Color.Orange;
                splitChecker.StartCheck(currentSplit);
            }
        }

        private void addNextButton_Click(object sender, EventArgs e)
        {
            Split split = new Split();
            hasNext = true;
            split.name = currentSplit.next == null ? "Next 1" : String.Format("Next {0}", currentSplit.next.Count() + 1);
            split.address = "";
            split.value = "";
            if (currentSplit.next == null)
            {
                currentSplit.next = new List<Split>();
            }
            currentSplit.next.Add(split);
            subSplitIndex = currentSplit.next.Count() - 1;
            subSplitView.Rows.Add(split.name);
            subSplitView.Rows[subSplitIndex].Selected = true;
        }

        private void subSplitAddress_TextChanged(object sender, EventArgs e)
        {
            if (hasNext == false && hasMore == false)
                return;
            Split subSplit = null;
            if (hasNext)
                subSplit = currentSplit.next[subSplitIndex];
            if (hasMore)
                subSplit = currentSplit.more[subSplitIndex];
            subSplit.address = subSplitAddress.Text;
        }

        private void subSplitTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (hasNext == false && hasMore == false)
                return;
            Split subSplit = null;
            if (hasNext)
                subSplit = currentSplit.next[subSplitIndex];
            if (hasMore)
                subSplit = currentSplit.more[subSplitIndex];
            subSplit.type = subSplitTypeComboBox.Text;

        }

        private void subSplitValue_TextChanged(object sender, EventArgs e)
        {
            if (hasNext == false && hasMore == false)
                return;
            Split subSplit = null;
            if (hasNext)
                subSplit = currentSplit.next[subSplitIndex];
            if (hasMore)
                subSplit = currentSplit.more[subSplitIndex];
            subSplit.value = subSplitValue.Text;
        }

        private void subSplitView_SelectionChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Selection changed");
            if (subSplitView.SelectedCells.Count == 0)
                return;
            Split subSplit = null;
            subSplitIndex = subSplitView.SelectedCells[0].RowIndex;
            Console.WriteLine("subsplit Row changed");
            if (hasNext)
                subSplit = currentSplit.next[subSplitIndex];
            if (hasMore)
                subSplit = currentSplit.more[subSplitIndex];
            subSplitAddress.Text = subSplit.address;
            subSplitTypeComboBox.Text = subSplit.type;
            subSplitValue.Text = subSplit.value;
        }
    }
}
