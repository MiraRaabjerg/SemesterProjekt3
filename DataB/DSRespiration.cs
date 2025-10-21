using System; 
using System.Collections.Generic; 
using System.Linq;
using System.Threading;// Giver adgang til tråd-funktioner som Thread.Sleep() – bruges til at vente mellem målinger
using System.Device.Gpio;
namespace DataB;

public class DSRespiration
{
    private readonly GpioController _gpio;
    private readonly int _dataPin;
    private readonly int _clockPin;
    private readonly List<int> _målinger = new();
    private readonly int _tærskel = 10000;

    public DSRespiration(int dataPin, int clockPin)
    {
        _gpio = new GpioController();
        _dataPin = dataPin;
        _clockPin = clockPin;

        _gpio.OpenPin(_dataPin, PinMode.Input);
        _gpio.OpenPin(_clockPin, PinMode.Output);
    }

    public void LæsSensor()
    {
        int værdi = LæsHX711();
        _målinger.Add(værdi);
        Console.WriteLine($"Måling: {værdi}");
    }

    private int LæsHX711()
    {
        // Vent på at dataPin går LOW
        while (_gpio.Read(_dataPin) == PinValue.High) { }

        int result = 0;

        // Læs 24 bits
        for (int i = 0; i < 24; i++)
        {
            _gpio.Write(_clockPin, PinValue.High);
            Thread.Sleep(1); // kort delay
            result = (result << 1) | (_gpio.Read(_dataPin) == PinValue.High ? 1 : 0);
            _gpio.Write(_clockPin, PinValue.Low);
            Thread.Sleep(1);
        }

        // Send 25. puls for at sætte gain (128)
        _gpio.Write(_clockPin, PinValue.High);
        Thread.Sleep(1);
        _gpio.Write(_clockPin, PinValue.Low);
        Thread.Sleep(1);

        // Konverter til signed int
        if ((result & 0x800000) != 0)
            result |= unchecked((int)0xFF000000);

        return result;
    }

    public int HentBinærSignal()
    {
        if (_målinger.Count == 0) return 0;
        return _målinger[^1] > _tærskel ? 1 : 0;
    }

    public bool ErEpisodeIGang(int varighedSekunder)
    {
        if (_målinger.Count < varighedSekunder) return false;
        var seneste = _målinger.TakeLast(varighedSekunder);
        return seneste.All(v => v > _tærskel);
    }

    public List<int> HentAlleMålinger() => _målinger;
}