using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using USB2SnesW;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace LiveSplit.UI.Components
{
    internal class SplitChecker
    {
        private DispatcherTimer checkTimer;
        private MainUI ui;
        private bool hasNext = false;
        private Split splitToCheck;
        private USB2SnesW.USB2SnesW usb2snes;
        public delegate void simpleFunction();
        public delegate void splitValueDelegate(Split sp, uint word);
        public splitValueDelegate onCheckedSplit;
        public simpleFunction onSplitOk;
        public simpleFunction onSplitNok;
        public simpleFunction onUsb2snesError;
        public bool isActivated = false;
        public SplitChecker(MainUI ui, USB2SnesW.USB2SnesW usb2snes)
        {
            checkTimer = new DispatcherTimer();
            checkTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            checkTimer.Tick += onCheckTimerElasped;
            this.ui = ui;
            this.usb2snes = usb2snes;
        }
        public void StartCheck(Split toCheck)
        {
            splitToCheck = toCheck;
            if (splitToCheck.next != null)
            {
                hasNext = true;
                splitToCheck.posToCheck = 0;
            } else
            {
                hasNext = false;
            }
            isActivated = true;
            checkTimer.Start();
        }
        public void StopCheck()
        {
            isActivated = false;
            checkTimer.Stop();
        }
        private async void onCheckTimerElasped(Object source, EventArgs e)
        {
            bool good;
            if (hasNext)
            {
                int oldpos = splitToCheck.posToCheck;
                good = await doCheckSplitWithNext(splitToCheck);
                if (good && oldpos == 0)
                { // The first one
                    ui.changeSubSplitStatus(0, Color.Orange);
                    return;
                }
                if (splitToCheck.posToCheck > 0)
                {
                    Console.WriteLine(String.Format("{0} :: {1}", splitToCheck.posToCheck, good));
                    if (good)
                        ui.changeSubSplitStatus(oldpos - 1, Color.Green);
                    else
                        ui.changeSubSplitStatus(oldpos - 1, Color.Orange);
                }
                if (good && splitToCheck.posToCheck == splitToCheck.next.Count + 1)
                {
                    onSplitOk.Invoke();
                    StopCheck();
                }

            }
            else
            {
                good = await checkSplit(splitToCheck);
                if (good)
                {
                    onSplitOk.Invoke();
                    StopCheck();
                }
                else
                {
                    onSplitNok.Invoke();
                }
            }
        }

        async Task<bool> doCheckSplitWithNext(Split split)
        {
            if (split.next == null)
            {
                return await checkSplit(split);
            }

            bool ok = false;
            if (split.posToCheck > 0)
            {
                ok = await checkSplit(split.next[split.posToCheck - 1]);
            }
            else
            {
                ok = await checkSplit(split);
            }
            if (ok)
            {
                split.posToCheck++;
            }
            return ok;
        }
        private async Task<bool> checkSplit(Split split)
        {
            var addressSizePairs = new List<Tuple<uint, uint>>();
            addressSizePairs.Add(new Tuple<uint, uint>(split.addressInt, 2));
            if (splitToCheck.more != null)
            {
                foreach (var moreSplit in split.more)
                {
                    addressSizePairs.Add(new Tuple<uint, uint>(moreSplit.addressInt, 2));
                }
            }
            List<byte[]> data = null;
            try
            {
                data = await usb2snes.GetAddress(addressSizePairs);
            }
            catch
            {
                Debug.WriteLine("doCheckSplit: Exception getting address");
                onUsb2snesError.Invoke();
                return false;
            }

            if ((null == data) || (data.Count != addressSizePairs.Count))
            {
                Debug.WriteLine("doCheckSplit: Get address failed to return result");
                onUsb2snesError.Invoke();
                return false;
            }
            uint value = (uint)data[0][0];
            uint word = value + ((uint)data[0][1] << 8);
            onCheckedSplit.Invoke(split, word);
            bool result = split.check(value, word);
            return result;
        }
    }
}
