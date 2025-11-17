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

        public DSRespiration(int dataPin, int clockPin)
        {
            Console.WriteLine("[HX] Opretter Hx711...");

            // Brug ctor'en som din Hx711-version har
            _hx = new Hx711(dataPin, clockPin);

            // Tænd/init HX711
            _hx.PowerUp();

            // Find privat metode GetNetWeight(int) via reflection
            _getNetWeight =
                _hx.GetType().GetMethod(
                    "GetNetWeight",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    binder: null,
                    types: new[] { typeof(int) },
                    modifiers: null
                ) ?? throw new InvalidOperationException("Kunne ikke finde GetNetWeight på Hx711.");
        }


        /// Læs én rå værdi fra HX711 (uden glidende gennemsnit, uden offset).

        public int ReadRaw()
        {
            return SafeRead();
        }


        /// Læs gennemsnit af flere rå værdier for mere stabilt signal.

        public double ReadRawAverage(int n = 5)
        {
            long sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum += SafeRead();
                Thread.Sleep(2);
            }

            return sum / (double)n;
        }

        private int SafeRead()
        {
            // Brug den private GetNetWeight(int numberOfReads)
            object? result = _getNetWeight.Invoke(_hx, new object[] { 1 });
            return Convert.ToInt32(result ?? 0);
        }

        public void Dispose()
        {
            try
            {
                _hx?.PowerDown();
            }
            catch
            {
            }

            try
            {
                _hx?.Dispose();
            }
            catch
            {
            }
        }
    }
} 
