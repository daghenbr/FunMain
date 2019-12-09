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
        const int DetectedDistance = 130;
        const int DistanceGrantLow = 40;
        const int DistanceGrantHigh = 50;

        // Create the serial port with basic settings
        private SerialPort detectorPort = new SerialPort("COM8", 115200, Parity.None, 8, StopBits.One);
        private SerialPort arduinoPort = new SerialPort("COM10",   9600, Parity.None, 8, StopBits.One);
        private Random random = new Random();

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

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Incoming Data:");

            // Attach a method to be called when there
            // is data waiting in the port's buffer
            detectorPort.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

            // Begin communications
            int wbs = arduinoPort.WriteBufferSize;
            arduinoPort.Open();
            detectorPort.Open();
            //br = new BinaryReader(port.BaseStream);

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
                }
            }
            else if (detectorMode == DetectorMode.Detecting)
            {
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                }
                Console.WriteLine("{0} {1}", distance, lowestDistance);
                TimeSpan dt = DateTime.Now - detectionTime;
                if (dt.TotalMilliseconds > 1000)
                {
                    if (lowestDistance >= DistanceGrantLow && lowestDistance <= DistanceGrantHigh)
                    {
                        arduinoPort.BaseStream.WriteByte(1);
                    }
                    else
                    {
                        arduinoPort.BaseStream.WriteByte(2);
                    }
                    Console.WriteLine("-----");
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


        private void button2_Click(object sender, EventArgs e)
        {
            SoundPlayer simpleSound = new SoundPlayer(@"c:\Windows\Media\chimes.wav");
            //SoundPlayer simpleSound = new SoundPlayer(@"c:\prog\FunMain\Audio\AccessGranted.mp3");
            simpleSound.Play();

            SoundControls soundControls = new SoundControls();
            //soundControls.PlaySound();
        }
    }

}
