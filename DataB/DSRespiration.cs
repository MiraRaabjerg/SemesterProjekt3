using System;
using System.Threading;
using System.Reflection;
using Iot.Device.Hx711;

namespace DataB
{

    /// Ren data-adgang til HX711: læser rå værdier fra ADC'en.
    /// Ingen glidende gennemsnit, ingen tærskler, ingen episode-logik.

    public sealed class DSRespiration : IDisposable
    {
        private readonly Hx711 _hx;
        private readonly MethodInfo _getNetWeight;

        //Initialiserer HX711 og finder intern metode til vægtlæsning.
        public DSRespiration(int dataPin, int clockPin)
        {

            // Initialiser HX711 med angivne GPIO-pins
            _hx = new Hx711(dataPin, clockPin);

            // Denne gør at HX711 går fra strømbesparende tilstand til aktiv tilstand
            _hx.PowerUp();

            // forsøger at finde en privat metode kaldet "GetWeight" på HX711‑objektet, som tager én int‑parameter.
            // Hvis den ikke findes, kastes en exception.
            _getNetWeight =
                _hx.GetType().GetMethod(
                    "GetNetWeight",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    binder: null,
                    types: new[] { typeof(int) },
                    modifiers: null
                ) ?? throw new InvalidOperationException("Kunne ikke finde GetNetWeight på Hx711.");
        }


        // Læser én rå værdi fra HX711 (uden glidende gennemsnit og uden offset).
        public int ReadRaw()
        {
            return SafeRead();
        }


        /// Læs gennemsnit af flere rå værdier for mere stabilt signal.
        public double ReadRawAverage(int numberOfReads)
        {
            long sum = 0;
            for (int i = 0; i < numberOfReads; i++)
            {
                sum += SafeRead();
                Thread.Sleep(2);
            }

            return sum / (double)numberOfReads;
        }

        private int SafeRead()
        {
            // Læser én måling fra HX711 ved at kalde en skjult (privat) funktion.
            // Dette er en midlertidig løsning (bør udskiftes med en public GetWeight metode)
            object? result = _getNetWeight.Invoke(_hx, new object[] { 1 });
            return Convert.ToInt32(result ?? 0);
        }

        //Slukker HX711 og frigiver ressourcer
        public void Dispose()
        {
            try
            {
                _hx?.PowerDown();
            }
            catch
            {
                /* Ignorerer fejl ved nedlukning */
            }

            try
            {
                _hx?.Dispose();
            }
            catch
            {
                /* Ignorerer fejl ved oprydning */
            }
        }
    }
} 
