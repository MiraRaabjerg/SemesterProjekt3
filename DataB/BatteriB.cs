namespace DataB;
using System.Device.Gpio; 

// Batteri-lag: læser spænding og konverterer til procent.
public class BatteriB:IBatteriB 
{
    private readonly int _adcPin; // Pin til analog spændingsmåling

    // Initialiserer med den pin, der bruges til ADC-måling
    public BatteriB(int adcPin) 
    {
        _adcPin = adcPin;
        // Initialiser ADC - HW
    }
    
    // Returnerer batteriniveau som procent (0–100%)
    public int HentBatteriProcent() 
    {
        int spænding = LæsSpænding(); // f.eks. 3700mV
        return KonverterSpændingTilProcent(spænding);
    }

    private int LæsSpænding() {
        // Læs spænding fra ADC HW (vi har sat værdien, da dette ikke er realiseret i HW)
        return 3700; // Vi har sat spændingen til 3700mV
    }

    private int KonverterSpændingTilProcent(int spænding) 
    {
        // Konverter fx 3.0V = 0%, 4.2V = 100%
        return Math.Clamp((spænding - 3000) * 100 / (4200 - 3000), 0, 100);
    }
}