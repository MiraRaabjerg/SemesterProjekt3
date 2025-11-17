// See https://aka.ms/new-console-template for more information

using System;
using System.Threading;
using BusinessLogicB;
using DataB;   // <- vigtigt, så DSRespiration findes

class Program
{
    static volatile bool _run = true;

    static void Main()
    {
        // GPIO (BCM-numre): ret til jeres faktiske pins
        int dataPin = 22;  // DOUT
        int clockPin = 27;  // SCK

        Console.CancelKeyPress += (s, e) => { e.Cancel = true; _run = false; };

        using var ds = new DSRespiration(dataPin, clockPin);
        var net = new Netværk(ds);        // giv netværkslaget adgang til målinger
        net.StartServer(5000);

        Console.WriteLine("Starter måling... (tryk Ctrl+C for at stoppe)\n");

        while (_run)
        {
            double v = ds.LæsSignal();                          // glattet værdi
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {v:F0}");
            Thread.Sleep(200);                                  // ~5 Hz
        }

        try { net?.StopServer(); } catch { }   // eller server.Stop() i Valg B
        Console.WriteLine("Lukker ned. Farvel!");
    }
}
