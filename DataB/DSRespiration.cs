using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HX711DotNet;

namespace DataB
{
    public class DSRespiration
    {
        private readonly HX711 _sensor;
        private readonly List<int> _målinger = new();
        private readonly int _tærskel = 10000;

        public DSRespiration(byte dataPin, byte clockPin)
        {
            _sensor = new HX711(dataPin, clockPin);

            // Reference unit bruges til at kalibrere vægten
            _sensor.SetReferenceUnit(1); // Justér efter din sensor

            _sensor.Reset();
            _sensor.Tare(); // Nulstil vægten
            Console.WriteLine("Sensor nulstillet. Klar til måling.");
        }

        public void LæsSignal()
        {
            try
            {
                int værdi = _sensor.GetWeight(5); // Læs gennemsnit af 5 målinger
                _målinger.Add(værdi);
                Console.WriteLine($"Måling: {værdi}");

                // Power cycle for stabilitet
                _sensor.PowerDown();
                _sensor.PowerUp();
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved læsning: {ex.Message}");
            }
        }

        public bool ErEpisodeIGang(int varighedSekunder)
        {
            if (_målinger.Count < varighedSekunder) return false;
            var seneste = _målinger.TakeLast(varighedSekunder);
            return seneste.All(v => v > _tærskel);
        }

        public List<int> HentAlleMålinger() => _målinger;
    }
}
