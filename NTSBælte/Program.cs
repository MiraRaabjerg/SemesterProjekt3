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

            // ===== Ctrl+C handler så vi kan lukke pænt ned =====
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("\n[CTRL+C] Stop-signal modtaget, lukker pænt ned...");
                e.Cancel = true;   // Forhindrer hårdt stop
                _run = false;      // Får while-løkke til at stoppe
            };

            try
            {
                // ===== Opret ADC (business-logic-laget) =====
                Console.WriteLine($"[INIT] Initialiserer ADC (DOUT={dataPin}, SCK={clockPin})...");
                using var adc = new ADC(dataPin, clockPin);

                // ===== Opret og start Netværk (TCP-server) =====
                Console.WriteLine($"[NET] Starter netværksserver på port {tcpPort}...");
                using var net = new Netværk(adc);      // klassen hedder Netvaerk / Netværk hos dig
                net.StartServer(tcpPort);
                Console.WriteLine("[NET] Netværksserver kører. Klar til GET_DATA-requests.\n");

                // ===== Måle-loop =====
                int count = 0;
                Console.WriteLine("Starter løbende målinger...\n");

                while (_run)
                {
                    // Læs glattet værdi via business-logic-laget
                    double v = adc.LæsSignal();    // eller LæsSignal() – brug dit faktiske navn
                    count++;

                    Console.WriteLine(
                        $"{DateTime.Now:HH:mm:ss.fff} Måling #{count}: {v:F0}");

                    // ~5 målinger i sekundet
                    Thread.Sleep(200);
                }

                Console.WriteLine("\nStopper måling og netværksserver...\n");

                // Når vi forlader try-blokken, kaldes Dispose() automatisk
                // på både adc og net (pga. using-deklarationerne).
                Console.WriteLine("[NET] Server stoppet.");
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
}
