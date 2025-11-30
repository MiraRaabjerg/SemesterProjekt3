namespace DataB;

public class LEDBData: ILEDB 
{
    private readonly int _gpioPin; // Bruges ikke, da dette ikke er realiseret
    private Timer _blinkTimer; //timer til blinkefunktionen
    private bool _erTændt;

    //GPIO er ikke sat
    public LEDBData(int gpioPin) 
    {
        _gpioPin = gpioPin;
        // Initialiser GPIO som output
    }

    public void Tænd() 
    {
        _erTændt = true;
        // GPIO HIGH
    }

    public void Sluk() 
    {
        _erTændt = false;
        // GPIO LOW
    }

    public void StartBlink() 
    {
        _blinkTimer = new Timer(_ => SkiftLED(), null, 0, 1000); // Blink hvert sekund
    }

    //Stopper blink og gendanner LED-status
    public void StopBlink() 
    {
        _blinkTimer?.Dispose();
        _blinkTimer = null;
        if (_erTændt) Tænd(); else Sluk();
    }

    //Skifter LED-status (tænd/sluk) – bruges af blink-timer.
    private void SkiftLED() 
    {
        if (_erTændt) Sluk(); else Tænd();
    }
}