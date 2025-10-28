// See https://aka.ms/new-console-template for more information

using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Threading;
using DataB;

class Program
{
    static void Main()
    {
        int dataPin = 5;   // GPIO-pin til DOUT
        int clockPin = 6;  // GPIO-pin til SCK

        using var ds = new DSRespiration(dataPin, clockPin);

        Console.WriteLine("Starter måling...\n");
        while (true)
        {
            double v = ds.LæsSignal();
            Thread.Sleep(200);   // 5 målinger i sekundet
        }
    }
}
