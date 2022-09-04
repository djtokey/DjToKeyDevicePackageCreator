using Ktos.DjToKey.Models;
using Midi.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DjToKey.DevicePackageCreator
{
    /// <summary>
    /// Interaction logic for MidiWindow.xaml
    /// </summary>
    public partial class MidiWindow : Window
    {
        private Midi.Devices.IInputDevice d;
        private bool autonewControl;
        public Device Device;
        public VisualControlsHandler ControlsHandler;

        private void autoNewControl(object sender, RoutedEventArgs e)
        {
            autonewControl = (sender as CheckBox).IsChecked ?? false;
        }

        private void D_ControlChange(ControlChangeMessage msg)
        {
            Dispatcher.Invoke(() =>
            {
                lbLog.Items.Add(string.Format("Cnt: {0} {1} {2}", msg.Channel, msg.Control, msg.Value));

                ControlsHandler.HighlightControl(((int)msg.Control).ToString());

                if (Device != null)
                {
                    var cc = Device.Controls.Find(x => x.ControlId == ((int)msg.Control).ToString());

                    if (autonewControl && cc == null)
                    {
                        var c = new ViewControl() { ControlId = ((int)msg.Control).ToString(), ControlName = msg.Control.ToString(), Type = Ktos.DjToKey.Plugins.Device.ControlType.Analog };
                        Device.Controls.Add(c);
                        ControlsHandler.SynchronizeView();
                    }
                }
            });
        }

        private void D_NoteOff(NoteOffMessage msg)
        {
            Dispatcher.Invoke(() =>
            {
                lbLog.Items.Add(string.Format("Off: {0} {1}", msg.Pitch, msg.Velocity));
            });
        }

        private void D_NoteOn(NoteOnMessage msg)
        {
            Dispatcher.Invoke(() =>
            {
                lbLog.Items.Add(string.Format("On : {0}({2}) {1}", msg.Pitch, msg.Velocity, (int)msg.Pitch));                

                if (autonewControl && Device.Controls.Find(x => x.ControlId == ((int)msg.Pitch).ToString()) == null)
                {
                    var c = new ViewControl() { ControlId = ((int)msg.Pitch).ToString(), ControlName = msg.Pitch.ToString(), Type = Ktos.DjToKey.Plugins.Device.ControlType.Analog };

                    Device.Controls.Add(c);
                    ControlsHandler.SynchronizeView();

                }
            });
        }

        private void D_ProgramChange(ProgramChangeMessage msg)
        {
            Dispatcher.Invoke(() =>
            {
                lbLog.Items.Add(string.Format("Pro: {0}", msg.Instrument));                
            });
        }

        private void MidiActive(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked ?? false)
            {
                if (inputDevices.SelectedIndex == -1)
                    return;

                d = Midi.Devices.DeviceManager.InputDevices[inputDevices.SelectedIndex];
                d.NoteOn += D_NoteOn;
                d.NoteOff += D_NoteOff;
                d.ProgramChange += D_ProgramChange;
                d.ControlChange += D_ControlChange;
                if (!d.IsOpen) d.Open();
                d.StartReceiving(null);
            }
            else
            {
                d.StopReceiving();
                d.Close();
            }
        }

        public MidiWindow()
        {
            InitializeComponent();

            foreach (var d in Midi.Devices.DeviceManager.InputDevices)
                inputDevices.Items.Add(d.Name);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lbLog.Items.Clear();
        }
    }
}
