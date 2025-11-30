using System;
using System.Threading;
using BusinessLogicB;   // ADC og Netværk ligger her

namespace NTSBaelte
{
    internal class Program
    {
        // Bruges til at styre om måleloopet skal køre eller stoppe (når man trykker Ctrl+C)
        private static volatile bool _run = true;

        private static void Main()
        {
            // Konfiguration af pins og port
            // dataPin = ben hvor HX711 sender måledata (DOUT)
            //clockPin = ben hvor HX711 får klokkesignal (SCK)
            const int dataPin = 5; // DOUT fra HX711
            const int clockPin = 6; // SCK   fra HX711
            const int tcpPort = 5000; // Port til TCP-server
            
            Console.WriteLine(" NTSBælte - måleprogram");
          

            Console.WriteLine($"Starter med GPIO: DOUT={dataPin}, SCK={clockPin}");
            Console.WriteLine("Tryk Ctrl+C for at stoppe programmet.\n");

            // Gør så Ctrl+C stopper løkken 
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("\nStop-signal modtaget, lukker ned...");
                e.Cancel = true;
                _run = false;
            };

            try
            {
                // initailisere ADC
                using var adc = new ADC(dataPin, clockPin);

                // Netværkslaget får adgang til ADC (så det kan sende målinger videre)
                Console.WriteLine("Starter netværksserver på port {tcpPort}.");
                var net = new Netværk(adc);
                net.StartServer(5000);
                Console.WriteLine("Netværksserver kører nu på port {tcpPort}.\n");

                Console.WriteLine("Starter løbende målinger...\n");
                
                int count = 0;

                while (_run)
                {
                    // Læs måling fra ADC
                    double v = adc.LæsSignal();
                    count++;
                    
                    //udskriver måling med tid og nummer
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Måling #{count}: {v}");

                    // vent 200ms (~5 målinger i sekundet)
                    Thread.Sleep(200);
                }

                Console.WriteLine("Program afsluttet");
            }
            catch (Exception ex)
            {
                //fejlbesked hvis noget går galt
                Console.WriteLine("Fejl i programmet");
                Console.WriteLine(ex.Message);
            }
        }
    }
}


