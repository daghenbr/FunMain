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
        // Create the serial port with basic settings
        private SerialPort port = new SerialPort("COM8",
          115200, Parity.None, 8, StopBits.One);
        private SerialPort arduinoPort = new SerialPort("COM10",
          9600, Parity.None, 8, StopBits.One);

        int byteNumber;
        int distanceLow;
        int distanceHigh;
        int strengthLow;
        int strengthHigh;
        int quality;
        BinaryReader br;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Incoming Data:");

            // Attach a method to be called when there
            // is data waiting in the port's buffer
            port.DataReceived += new
              SerialDataReceivedEventHandler(port_DataReceived);

            // Begin communications
            int wbs = arduinoPort.WriteBufferSize;
            arduinoPort.Open();
            port.Open();
            br = new BinaryReader(port.BaseStream);

        }



        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (port.BytesToRead > 0)
            {
                if (byteNumber == 0)
                {
                    int a = port.ReadByte();
                    if (a == 89)
                        byteNumber++;
                }
                if (byteNumber == 1)
                {
                    int a = port.ReadByte();
                    if (a == 89)
                        byteNumber++;
                    else
                        byteNumber = 0;
                }
                if (byteNumber == 2)
                {
                    distanceLow = port.ReadByte();
                    byteNumber++;
                }
                if (byteNumber == 3)
                {
                    distanceHigh = port.ReadByte();
                    byteNumber++;
                }
                if (byteNumber == 4)
                {
                    strengthLow = port.ReadByte();
                    byteNumber++;
                }
                if (byteNumber == 5)
                {
                    strengthHigh = port.ReadByte();
                    byteNumber++;
                }
                if (byteNumber == 6)
                {
                    port.ReadByte();
                    byteNumber++;
                }
                if (byteNumber == 7)
                {
                    quality = port.ReadByte();
                    byteNumber++;
                }
                if (byteNumber == 8)
                {
                    port.ReadByte();
                    byteNumber = 0;
                    Console.WriteLine("{0} {1} {2} {3}", distanceLow + distanceHigh * 256, strengthLow + strengthHigh * 256, quality, arduinoPort.BytesToWrite);
                    int distance = distanceLow + distanceHigh * 256;
                    if (distance < 40)
                        arduinoPort.BaseStream.WriteByte(1);
                    else if(distance < 50)
                        arduinoPort.BaseStream.WriteByte(2);
                }
            }
            return;
                // Show all the incoming data in the port's buffer
                var existing = port.ReadExisting();

            //Console.WriteLine("{0} {1}", existing, existing.Length);
            if (existing.Length > 5)
            {

                int a = existing[0];
                int b = existing[1];
                int c = existing[2];
                int d = existing[3];
                Console.WriteLine("{0} {1} {2} {3}", a, b, c, d);
            }
            //using (BinaryReader br = new BinaryReader(port.BaseStream))
            if (false)
            {
                //while (true)
                {
                    if (port.BytesToRead > 16)
                    {
                        while (br.ReadByte() != 89 && br.ReadByte() != 89)
                        {

                        }
                        //var header1 = br.ReadByte();
                        //var header2 = br.ReadByte();
                        var height1 = br.ReadByte();
                        var height2 = br.ReadByte();
                        Console.WriteLine("{0}", (height1 + height2 * 256) / 1000);
                        //br.ReadBytes(5);
                    }
                    System.Threading.Thread.Sleep(1000);
                    port.ReadExisting();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SoundPlayer simpleSound = new SoundPlayer(@"c:\Windows\Media\chimes.wav");
            simpleSound.Play();

        }
    }

}
