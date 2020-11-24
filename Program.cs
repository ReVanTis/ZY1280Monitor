using System;
using System.IO.Ports;
using System.Linq;

namespace ZY1280Monitor
{
    class Program
    {
        public static int ZY1280BaudRate = 115200;
        public static string ZY1280PortNum = "";
        static void Main(string[] args)
        {
            if ( Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                Console.WriteLine($"Running on {Environment.OSVersion}");
                Console.WriteLine(@"Warning: Only Win32 is tested.
You may experience unexpected behaviors on other platforms.
Enter 'Yes' to continue to use.");
                var line = Console.ReadLine();
                if (line != "Yes") return;
            }
            var portList = SerialPort.GetPortNames();
            if (args.Length < 1)
            {
                Console.WriteLine("Need to provide COM port number.\nCurrent available ports are:");
                if (portList.Length > 10)
                {
                    foreach (var p in portList.Take(10))
                        Console.WriteLine(p);
                    Console.WriteLine("...");
                }
                else
                {
                    foreach (var p in portList)
                        Console.WriteLine(p);
                }
                return;
            }

            if (args.Length >= 1)
                ZY1280PortNum = args[0];
            if (args.Length >= 2)
            {
                if(!int.TryParse(args[1], out ZY1280BaudRate))
                {
                    Console.WriteLine("Baudrate invalid. Using default 115200...");
                    ZY1280BaudRate = 115200;
                }
            }

            SerialPort ZY1280Port = new(ZY1280PortNum, ZY1280BaudRate);
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            try
            {
                ZY1280Port.Open();
                while(true)
                {
                    int bcount = 0;
                    byte[] buffer = new byte[32];
                    bcount = (byte) ZY1280Port.Read(buffer,0,32);

                    if (bcount == 1 && buffer[0] == 0xAB)
                    {
                        //Console.WriteLine("0xAB magic found.");
                    }
                    else if (bcount == 27)
                    {
                        byte b = 0xAB;
                        for (int i = 0; i < 26; i++)
                        {
                            b = (byte)(b + buffer[i]);
                        }

                        if (b != buffer[26])
                        {
                            Console.WriteLine("CRC failure.");
                            ZY1280Port.DiscardInBuffer();
                            continue;
                        }

                        double V   = BitConverter.ToInt32 (buffer, 2)  / 10000D;
                        double A   = BitConverter.ToInt32 (buffer, 6)  / 10000D;
                        double A_H = BitConverter.ToUInt32(buffer, 10) / 10000D;
                        double W_H = BitConverter.ToUInt32(buffer, 14) / 10000D;
                        int T_ms = (int)BitConverter.ToUInt32(buffer, 18) * 10;
                        TimeSpan T = new(0, 0, 0, 0, T_ms);
                        double D_N = BitConverter.ToUInt16(buffer, 22) / 1000D;
                        double D_P = BitConverter.ToUInt16(buffer, 24) / 1000D;
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine($"V : {V:0.00000}");
                        Console.WriteLine($"A : {A:0.00000}");
                        Console.WriteLine($"W : {(V * A):0.00000}");
                        Console.WriteLine($"D+: {D_P:0.000}");
                        Console.WriteLine($"D-: {D_N:0.000}");
                        Console.WriteLine($"AH: {A_H:0.00000}");
                        Console.WriteLine($"WH: {W_H:0.00000}");
                        Console.WriteLine($"T : {T:hh\\:mm\\:ss\\.ff}");
                    }
                    else
                    {
                        Console.WriteLine("NOT synced.");
                        ZY1280Port.DiscardInBuffer();
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
            finally
            {
                if(ZY1280Port.IsOpen)
                    ZY1280Port.Close();
            }
        }
    }
}
