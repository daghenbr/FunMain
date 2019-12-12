using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Media;

namespace FunMain
{
    public partial class Form1 : Form
    {
        const int DetectedDistance = 130; //Start measuring when distance is less than this
        const int DistanceGrantLow = 64;  //Lowest accepted distance
        const int DistanceGrantHigh = 79; //Highest accepted distance

        int distanceGrantLow = DistanceGrantLow;
        int distanceGrantHigh = DistanceGrantHigh;

        // Create the serial port with basic settings
        private SerialPort detectorPort = new SerialPort("COM8", 115200, Parity.None, 8, StopBits.One);
        private SerialPort arduinoPort = new SerialPort("COM10",   9600, Parity.None, 8, StopBits.One);
        //private Random random = new Random();
        SoundControls soundControls = new SoundControls();

        enum DetectorMode
        {
            Waiting,
            Detecting,
            Pause
        }

        int byteNumber;
        int distanceLow;
        int distanceHigh;
        int strengthLow;
        int strengthHigh;
        int quality;
        //BinaryReader br;

        DetectorMode detectorMode = DetectorMode.Waiting;
        //bool detectingNow = false;
        DateTime detectionTime = new DateTime(0);
        int lowestDistance;
        int previousDistance;
        public Form1()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Start communication with optical sensor and Arduino.
        /// This method is called from the form load event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartCommunication()
        {
            // Attach a method to be called when there
            // is data waiting in the port's buffer
            detectorPort.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

            // Begin communications
            int wbs = arduinoPort.WriteBufferSize;
            arduinoPort.Open();
            detectorPort.Open();
        }



        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (detectorPort.BytesToRead > 0)
            {
                if (byteNumber == 0)
                {
                    int a = detectorPort.ReadByte();
                    if (a == 89)
                        byteNumber++;
                }
                if (byteNumber == 1)
                {
                    int a = detectorPort.ReadByte();
                    if (a == 89)
                        byteNumber++;
                    else
                        byteNumber = 0;
                }
                if (byteNumber == 2)
                {
                    distanceLow = detectorPort.ReadByte();
                    byteNumber++;
                }
                if (byteNumber == 3)
                {
                    distanceHigh = detectorPort.ReadByte();
                    byteNumber++;
                }
                if (byteNumber == 4)
                {
                    strengthLow = detectorPort.ReadByte();
                    byteNumber++;
                }
                if (byteNumber == 5)
                {
                    strengthHigh = detectorPort.ReadByte();
                    byteNumber++;
                }
                if (byteNumber == 6)
                {
                    detectorPort.ReadByte();
                    byteNumber++;
                }
                if (byteNumber == 7)
                {
                    quality = detectorPort.ReadByte();
                    byteNumber++;
                }
                if (byteNumber == 8)
                {
                    detectorPort.ReadByte(); //Checksum
                    byteNumber = 0;
                    int distance = distanceLow + distanceHigh * 256;
                    int strength = strengthLow + strengthHigh * 256;
                    //Console.WriteLine("{0} {1} {2} {3}", distance, strength, quality, arduinoPort.BytesToWrite);
                    CheckMeasuredDistance(distance);
                }
            }
        }

        private void CheckMeasuredDistance(int distance)
        {
            if (detectorMode == DetectorMode.Waiting)
            {
                if (distance < DetectedDistance)
                {
                    detectorMode = DetectorMode.Detecting;
                    detectionTime = DateTime.Now;
                    lowestDistance = distance;
                    arduinoPort.BaseStream.WriteByte(3);
                }
            }
            else if (detectorMode == DetectorMode.Detecting)
            {
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                }
                TimeSpan dt = DateTime.Now - detectionTime;
                if (dt.TotalMilliseconds > 1000)
                {
                    //Console.WriteLine("{0}", lowestDistance);
                    Action<string> AddLine = AddToListBox;
                    listBox1.Invoke(AddLine, new object[] { lowestDistance.ToString() });
                    if (lowestDistance >= distanceGrantLow && lowestDistance <= distanceGrantHigh)
                    {
                        arduinoPort.BaseStream.WriteByte(1);
                        soundControls.PlayGranted();
                    }
                    else
                    {
                        arduinoPort.BaseStream.WriteByte(2);
                        soundControls.PlayDenied();
                    }
                    //Console.WriteLine("-----");
                    detectorMode = DetectorMode.Pause;
                }
            }
            else
            {
                TimeSpan dt = DateTime.Now - detectionTime;
                if (dt.TotalMilliseconds > 4000)
                {
                    arduinoPort.BaseStream.WriteByte(0);
                    detectorMode = DetectorMode.Waiting;
                }

            }
        }


        /// <summary>
        /// Not in use now.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            SoundControls soundControls = new SoundControls();
            soundControls.PlayAccessGranted();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            numericUpDownLow.Value = DistanceGrantLow;
            numericUpDownHigh.Value = DistanceGrantHigh;
            timer1.Start();
            StartCommunication();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            distanceGrantLow = (int)numericUpDownLow.Value;
            distanceGrantHigh = (int)numericUpDownHigh.Value;
        }


        /// <summary>
        /// Add a line to the ListBox, in a thread-safe manner, through Invoke.
        /// </summary>
        /// <param name="str"></param>
        private void AddToListBox(string str)
        {
            listBox1.Items.Add(str);
            listBox1.TopIndex = listBox1.Items.Count - 1; //Scroll to bottom
        }

    }

}
