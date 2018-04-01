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
        static void Main()
        {
            ip = "http://192.168.2.48"; //MagicMirror2 IP Address
            port = "8080"; //MagicMirror2 Port
            var COMPort = "COM4"; //COM Port of the USB Modem. Look in device Manager in windows to locate this info
            try
            {
                Sp = new SerialPort(COMPort, 9600, Parity.None, 8, StopBits.One) { DtrEnable = true };

                Sp.Open();

                Sp.WriteLine("AT#CID=1" + Environment.NewLine);
                Thread.Sleep(1000);

                Sp.DataReceived += sp_DataReceived;
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
            Console.ReadLine();
        }

        static void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            var currentName = "";
            var currentNumber = "";
            try
            {

                Thread.Sleep(500);

                string x = Sp.ReadExisting();


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

                    if (x.ToLower().Contains("ring") &&
                        currentName != string.Empty && currentNumber != string.Empty)
                    {
                        Console.WriteLine(currentName);
                        try
                        {
                            var urlAlert = string.Format("{0}:{1}/api/v1/modules/alert/SHOW_ALERT?message={2}\n{3}&title=Incoming Phone Call", ip, port, currentName, currentNumber);

                            WebRequest showPhoneNumberOnScreen = WebRequest.Create(urlAlert);
                            showPhoneNumberOnScreen.Method = "POST";
                            showPhoneNumberOnScreen.GetResponse();
                        }
                        catch { }

                        //Give the users enough time to look at the magic mirror to see who is calling.
                        Thread.Sleep(8500); //That seems like enough time to glace at a name ... ???
                        //Then remove the Alert on the mirrors screen
                        string urlCloseAlert = string.Format("{0}:{1}/api/v1/modules/alert/HIDE_ALERT", ip, port);
                        WebRequest removePhoneNumberOnScreen = WebRequest.Create(urlCloseAlert);
                        removePhoneNumberOnScreen.Method = "POST";
                        removePhoneNumberOnScreen.GetResponse();


                    }
                }
            }
            catch
            {

            }

        }
    }
}




