using System;
using System.IO.Ports;
using System.Net;
using System.Threading;

namespace HomePhoneUSBModemController
{
    class Program
    {
        private static SerialPort Sp;
        private static string ip { get; set; }
        private static string port { get; set; }
        private static string COMPort = "";
        static void Main()
        {
            ip = "http://192.168.2.48"; //MagicMirror2 IP Address
            port = "8080"; //MagicMirror2 Port
            COMPort = "COM4"; //COM Port of the USB Modem. Look in device Manager in windows to locate this info
            Console.WriteLine("Creating new serial port interface ...");
            try
            {
                Sp = new SerialPort(COMPort, 9600, Parity.None, 8, StopBits.One) {DtrEnable = true};


                Sp.Open();

                Sp.WriteLine("AT#CID=1" + Environment.NewLine);
                Thread.Sleep(1000);

                Sp.DataReceived += sp_DataReceived;
                //Sp.Disposed +=Sp_Disposed;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            } 

            Console.WriteLine("Ready for phone call...\n");
            Console.ReadLine();
        }


        static void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            var currentName = "";
            var currentNumber = "";
            

                Thread.Sleep(500);

                string x = Sp.ReadExisting();

                try
                {
                string rString = x.Replace("\r\n", " ");
                string[] lines = rString.Split(' ');

                for (int i = 0; i <= lines.Length; i++)
                {
                    if (lines[i].Contains("NAME="))
                    {
                        currentName = (lines[i].Replace("NAME=", "") + " " + lines[i + 1]);
                        if (currentName.ToLower() == "p ")
                        {
                            currentName = "Private Caller";
                        }
                        if (currentName.ToLower() == "o ")
                        {
                            currentName = "Private Caller";
                        }


                    }

                    if (lines[i].Contains("NMBR="))
                    {
                        var num = (lines[i].Replace("NMBR=", ""));
                        currentNumber = num;
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
                if (x.ToLower().Contains("ring") &&
                        currentName != string.Empty && currentNumber != string.Empty)
                    {
                        Console.WriteLine(currentName);
                        try
                        {
                            var urlAlert =
                                string.Format(
                                    "{0}:{1}/api/v1/modules/alert/SHOW_ALERT?timer=8500&message={2}\n{3}&title=Incoming Phone Call",
                                    ip, port, currentName, currentNumber);
                           
                            var request = (HttpWebRequest)WebRequest.Create(urlAlert);
                            request.Method = "POST";
                            request.KeepAlive = false;
                            var response = (HttpWebResponse)request.GetResponse();
                            Console.Write("Sent to MagicMirror: " + response.StatusDescription + "\n");
                              
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                   
                    }
            Sp.DiscardInBuffer();


        }
    }
}




