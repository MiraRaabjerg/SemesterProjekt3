namespace DataB;
using System.Device.Gpio; 

public class BatteriB:IBatteriB 
{
    private readonly int _adcPin;

    public BatteriB(int adcPin) 
    {
        _adcPin = adcPin;
        // Initialiser ADC
    }

    public int HentBatteriProcent() 
    {
        int spænding = LæsSpænding(); // fx 3700 mV
        return KonverterSpændingTilProcent(spænding);
    }

    private int LæsSpænding() {
        // Læs spænding fra ADC
        return 3700; // placeholder
    }

    private int KonverterSpændingTilProcent(int spænding) 
    {
        // Konverter fx 3.0V = 0%, 4.2V = 100%
        return Math.Clamp((spænding - 3000) * 100 / (4200 - 3000), 0, 100);
    }
}