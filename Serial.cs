using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;

namespace SerialReader
{
    public partial class Serial : Form
    {
        private SerialPort MySerial;
        private bool DeviceConnected;

        public Serial()
        {
            InitializeComponent();

            MySerial = new SerialPort();
            RefreshSerialPorts();    

            textBoxMessageIn.ScrollBars = ScrollBars.Vertical;
        }

        ~Serial()
        {
            
        }

        private void Form1_FormClosing(object sender, EventArgs e)
        {
            if (MySerial.IsOpen)
                MySerial.Close();
        }

        private void RefreshSerialPorts()
        {
            // Serial port detection
            string[] ports = SerialPort.GetPortNames();

            comboBoxPort.Items.Clear();
            comboBoxPort.Text = "(none)";

            // Check if there's serial port readys for communication
            if (ports.Length <= 1) {
                buttonStart.Enabled = false;
                buttonStop.Enabled = false;

                MessageBox.Show
                (
                    "No open Serial Ports detected. Check your device's connection and/or drivers.",
                    "No Serial Ports", MessageBoxButtons.OK, MessageBoxIcon.Error
                );
                DeviceConnected = false;
            }

            Console.Write("Serial ports ready: ");

            foreach (string port in ports)
            {
                Console.Write("{0} ", port);
                comboBoxPort.Items.Add(port);
            }
            Console.WriteLine();
            comboBoxPort.SelectedIndex = 0;

            MySerial.BaudRate = 9600;
            MySerial.DtrEnable = true;

            DeviceConnected = true;

            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
        }

        // Method for capturing any message received through the serial port
        private void MySerial_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            String rxString = MySerial.ReadLine();
            this.BeginInvoke(new LineReceivedEvent(LineReceived), rxString);
        }

        private delegate void LineReceivedEvent(string POT);

        private void LineReceived(string POT)
        {
            //  Split values
            //  String[] DataReceived = POT.Split(',');

            textBoxMessageIn.AppendText(POT + "\n");
        }
        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (DeviceConnected)
            {
                MySerial.PortName = comboBoxPort.Text;
                MySerial.Open();
                
                // Add event for capturing serial messages
                MySerial.DataReceived += MySerial_DataReceived;
            }

            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
            buttonReload.Enabled = false;

            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            refreshSerialPortsToolStripMenuItem.Enabled = false;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (DeviceConnected)
            {
                if (MySerial.IsOpen)
                    MySerial.Close();

                MySerial.DataReceived -= MySerial_DataReceived; 
            }

            buttonStart.Enabled = true;
            buttonStop.Enabled = false;

            buttonReload.Enabled = true;

            startToolStripMenuItem.Enabled = true;
            stopToolStripMenuItem.Enabled = false;
            refreshSerialPortsToolStripMenuItem.Enabled = true;
        }

        private void buttonReload_Click(object sender, EventArgs e)
        {
            RefreshSerialPorts();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (DeviceConnected)
            {
                MySerial.Write(textBoxMessageOut.Text);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            saveFileDialog.FileName = unixTimestamp.ToString();
            saveFileDialog.Filter = "Text File (*.txt)|*.txt|Comma Separated Values (*.csv)|*.csv|Data File (*.dat)|*.dat|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8))
                {
                    sw.WriteLine(textBoxMessageIn.Text);
                }
            }
        }

        private void clearMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBoxMessageIn.Clear();
        }

        private void textBoxMessageOut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonSend_Click(sender, e);
            }
        }
     }
}
