// http://www.nudoq.org/#!/Projects/32feet.NET
// http://stackoverflow.com/questions/16802791/pair-bluetooth-devices-to-a-computer-with-32feet-net-bluetooth-library

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Windows.Forms;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

namespace SurfaceApplication2
{
    class BluetoothManager
    {
        // Default addresses
        private static Guid UUID = new Guid("{00001101-0000-1000-8000-00805F9B34FB}");
        private static string LOCAL_ADDRESS = "90:A4:DE:A1:82:98";
        private static string DEVICE_ADDESS = "30:76:6F:7E:F5:C6";
        private static string PAIRING_PIN = "0000";

        private BluetoothEndPoint EP;
        private BluetoothClient BC;

        // Remote device that would connect (direct connection, no scanning)
        private BluetoothDeviceInfo BTDevice;

        private NetworkStream stream = null;

        public BluetoothManager()
        {
            EP = new BluetoothEndPoint(BluetoothAddress.Parse(LOCAL_ADDRESS), BluetoothService.SerialPort);
            BC = new BluetoothClient(EP);
            BTDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(DEVICE_ADDESS));
            stream = null;
        }

        public void Send(String str)
        {
            // Ask for pairing
            if (BluetoothSecurity.PairRequest(BTDevice.DeviceAddress, PAIRING_PIN))
            {
                // Check if device is authentificated
                if (BTDevice.Authenticated)
                {
                    // Pair the devices
                    BC.SetPin(PAIRING_PIN);

                    // Validate the connexion, and notify the callback function
                    BC.BeginConnect(BTDevice.DeviceAddress, BluetoothService.SerialPort, new AsyncCallback(Connect), BTDevice);
                }
            }
        }
        // Main function
        public void DoEverything()
        {
            // Ask for pairing
            if (BluetoothSecurity.PairRequest(BTDevice.DeviceAddress, PAIRING_PIN))
            {
                // Notification
                Console.WriteLine("PairRequest: OK");

                // Check if device is authentificated
                if (BTDevice.Authenticated)
                {
                    Console.WriteLine("Authenticated: OK - " + BTDevice.DeviceName);

                    // Pair the devices
                    BC.SetPin(PAIRING_PIN);

                    // Validate the connexion, and notify the callback function
                    BC.BeginConnect(BTDevice.DeviceAddress, BluetoothService.SerialPort, new AsyncCallback(ConnectTest), BTDevice);
                }
                else
                {
                    Console.WriteLine("Authenticated: No");
                }
            }
            else
            {
                Console.WriteLine("PairRequest: No");
            }

            // End notification 
            Console.WriteLine("\n- THE END -");
            Console.WriteLine("Please type a key to end the application.");
            Console.ReadLine();
        }


        // Callback function is connexion has been made
        private void Connect(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                // Client is connected now :)
                Console.WriteLine("BC.Connected? " + BC.Connected);

                //WRITE
                if (BC.Connected)
                {
                    Console.WriteLine("BC.RemoteMachineName? " + BC.RemoteMachineName);

                    // Get stream
                    stream = BC.GetStream();

                    if (stream != null)
                    {
                        /*
                        // write the data in the stream 
                        var buffer = System.Text.Encoding.UTF8.GetBytes("Pikachu");
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Flush();
                        stream.Close();
                        Console.WriteLine("Message sent");
                        */

                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        int i = 0;
                        while (i < 5)
                        {
                            if (sw.ElapsedMilliseconds > 3000)
                            {
                                // write the data in the stream 
                                String message = "Pikachu - " + i;
                                var buffer = System.Text.Encoding.UTF8.GetBytes(message);
                                stream.Write(buffer, 0, buffer.Length);
                                Console.WriteLine("Message #" + i + " sent");
                                i++;
                                sw = Stopwatch.StartNew();
                            }
                        }

                        stream.Flush();
                        stream.Close();

                    }
                }

                /*
                // READ
                // If a message is present, handle the reading (@FRED: not my code within this control structure)
                if (stream.CanRead)
                {

                    byte[] myReadBuffer = new byte[1024];
                    StringBuilder myCompleteMessage = new StringBuilder();
                    int numberOfBytesRead = 0;

                    // Incoming message may be larger than the buffer size. 
                    do
                    {
                        numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);

                        myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                    }
                    while (stream.DataAvailable);

                    // Print out the received message to the console.
                    Console.WriteLine("You received the following message : " + myCompleteMessage);
                }
                else
                {
                    Console.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                }
                */

                Console.ReadLine();
            }
        }

        private void ConnectTest(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                // Client is connected now :)
                Console.WriteLine("BC.Connected? " + BC.Connected);

                //WRITE
                if (BC.Connected)
                {
                    Console.WriteLine("BC.RemoteMachineName? " + BC.RemoteMachineName);

                    // Get stream
                    stream = BC.GetStream();

                    if (stream != null)
                    {
                        /*
                        // write the data in the stream 
                        var buffer = System.Text.Encoding.UTF8.GetBytes("Pikachu");
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Flush();
                        stream.Close();
                        Console.WriteLine("Message sent");
                        */

                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        int i = 0;
                        String[] str = {"a7a5", "a5d5", "b7b4", "b4e4", "d5e5", "e4d4", "h7h6", "h6g6", "a8a1", "h8h1", "a1a4", "a8a5", "b8c6", "e7e6", "f8c5"};
                        while (i < 15)
                        {
                            if (sw.ElapsedMilliseconds > 3000)
                            {
                                // write the data in the stream 
                                String message = str[i];
                                var buffer = System.Text.Encoding.UTF8.GetBytes(message);
                                stream.Write(buffer, 0, buffer.Length);
                                Console.WriteLine("Message #" + i + " sent");
                                i++;
                                sw = Stopwatch.StartNew();
                            }
                        }

                        stream.Flush();
                        stream.Close();

                    }
                }
                Console.ReadLine();
            }
        }


    }

    
}
