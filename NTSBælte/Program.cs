// See https://aka.ms/new-console-template for more information

using System;
using System.Threading;
using BusinessLogicB;  // ADC-klassen
// using DataB;       // behøves ikke længere her, ADC bruger DSRespiration internt

class Program
{
    // Bruges til at stoppe while-løkken pænt med Ctrl+C
    static volatile bool _run = true;

    static void Main()
    {
        // GPIO (BCM-numre) – ret til de pins I FAKTISK har forbundet
        int dataPin  = 5;  // DOUT fra HX711
        int clockPin = 6;  // SCK  fra HX711

        Console.WriteLine("===========================================");
        Console.WriteLine(" NTSBælte – ADC / HX711 test ");
        Console.WriteLine("===========================================\n");

        Console.WriteLine($"Starter med GPIO (BCM): DOUT={dataPin}, SCK={clockPin}");
        Console.WriteLine("Tryk Ctrl+C for at stoppe programmet.\n");

        // Gør så Ctrl+C stopper løkken 
        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("\n[CTRL+C] Stop-signal modtaget, lukker pænt ned...");
            e.Cancel = true;
            _run = false;
        };

        try
        {
            // ADC = business-logic-lag oven på DataB.DSRespiration
            Console.WriteLine("[INIT] Initialiserer ADC og HX711...");
            using var adc = new ADC(dataPin, clockPin);

            // Netværkslaget får adgang til ADC (så det kan sende målinger videre)
            Console.WriteLine("Starter netværksserver på port 5000...");
            var net = new Netværk(adc);   // <-- sørg for at Netværk har en ctor der tager ADC
            net.StartServer(5000);
            Console.WriteLine("Netværksserver kører nu på port 5000.\n");

            Console.WriteLine("Starter løbende målinger...\n");

            int count = 0;
            var start = DateTime.Now;

            while (_run)
            {
                // Læs glattet værdi via business-logic-laget
                double v = adc.LæsSignal();
                count++;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Måling #{count}: {v}");

                Thread.Sleep(200); // ~5 målinger i sekundet
            }

            Console.WriteLine("Stopper måling og netværksserver...");

            try
            {
                net.StopServer();
                Console.WriteLine("Server stoppet.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved StopServer: {ex.Message}");
            }

            Console.WriteLine("Program afsluttet. Farvel!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Der opstod en uventet fejl i Main:");
            Console.WriteLine(ex);
            Console.WriteLine("Tryk Enter for at lukke.");
            Console.ReadLine();
        }
    }
}

