using System; 
using System.Collections.Generic; 
using System.Linq;
using System.Threading;// Giver adgang til tråd-funktioner som Thread.Sleep() – bruges til at vente mellem målinger
using HX711DotNet;// Importerer HX711DotNet-biblioteket, som bruges til at læse data fra HX711-sensoren
using Unosquare.RaspberryIO;// Giver adgang til Raspberry Pi's GPIO-pins via C# – bruges til at styre hardware
using Unosquare.WiringPi;// Initialiserer GPIO-systemet på Raspberry Pi – kræves før du bruger pins

namespace DataB;

public class DSRespiration
{
    private readonly HX711 _sensor;              // HX711 sensor-objekt
    private readonly List<int> _målinger = new(); // Liste til at gemme målinger
    private readonly int _tærskel = 10000;        // Tærskelværdi for NT-episode

    // Constructor: initialiserer GPIO og HX711 med angivne pins
    public DSRespiration(int dataPin, int clockPin) 
    {
        Pi.Init<BootstrapWiringPi>();            // Starter GPIO-systemet
        _sensor = new HX711(dataPin, clockPin);  // Opretter HX711 med dine GPIO-pins
    }

    // Læser én værdi fra HX711 og gemmer den i listen
    public void LæsSignal() 
    {
        int værdi = _sensor.Read();              // Læs analog værdi fra respirationssensor
        _målinger.Add(værdi);                    // Gem målingen
        Console.WriteLine($"Måling: {værdi}");   // Udskriv til konsol
    }

    // Returnerer 1 hvis sidste måling er over tærskel, ellers 0
    public int HentBinærSignal() 
    {
        if (_målinger.Count == 0) return 0;
        return _målinger.Last() > _tærskel ? 1 : 0;
    }

    // Tjekker om der har været NT-episode i X sekunder
    public bool ErEpisodeIGang(int varighedSekunder) 
    {
        if (_målinger.Count < varighedSekunder) return false;

        var seneste = _målinger.TakeLast(varighedSekunder); // Hent de seneste målinger
        return seneste.All(v => v > _tærskel);               // Er de alle over tærskel?
    }

    // Returnerer alle målinger (til logning eller visualisering)
    public List<int> HentAlleMålinger() => _målinger;
}
}