using System;
using System.Threading;
using BusinessLogicB;   // ADC og Netværk ligger her

namespace NTSBaelte
{
    internal class Program
    {
        // Bruges til at stoppe while-løkke pænt med Ctrl+C
        private static volatile bool _run = true;

        private static void Main()
        {
            // ===== HW-konfiguration (BCM-numre) =====
            // Ret disse pins, så de matcher din faktiske tilslutning
            const int dataPin = 5;    // DOUT fra HX711
            const int clockPin = 6;   // SCK   fra HX711
            const int tcpPort = 5000; // Port til TCP-server

            Console.WriteLine("===================================================");
            Console.WriteLine(" NTSBælte – ADC / HX711 måleprogram");
            Console.WriteLine("===================================================\n");

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
                // ===== Opret ADC (business-logic-laget) =====
                Console.WriteLine($"[INIT] Initialiserer ADC (DOUT={dataPin}, SCK={clockPin})...");
                using var adc = new ADC(dataPin, clockPin);

            // Netværkslaget får adgang til ADC (så det kan sende målinger videre)
            Console.WriteLine("Starter netværksserver på port 5000...");
            var net = new Netværk(adc);   // <-- sørg for at Netværk har en ctor der tager ADC
            net.StartServer(5000);
            Console.WriteLine("Netværksserver kører nu på port 5000.\n");

            Console.WriteLine("Starter løbende målinger...\n");

                // ===== Måle-loop =====
                int count = 0;
                Console.WriteLine("Starter løbende målinger...\n");

                while (_run)
                {
                    // Læs glattet værdi via business-logic-laget
                    double v = adc.LæsSignal();    // eller LæsSignal() – brug dit faktiske navn
                    count++;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Måling #{count}: {v}");

                    // ~5 målinger i sekundet
                    Thread.Sleep(200);
                }

                Console.WriteLine("\nStopper måling og netværksserver...\n");

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

