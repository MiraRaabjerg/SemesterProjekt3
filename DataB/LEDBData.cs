namespace DataB;

public class LEDBData: ILEDB {
    private readonly int _gpioPin;
    private Timer _blinkTimer;
    private bool _erTændt;

    public LEDBData(int gpioPin) {
        _gpioPin = gpioPin;
        // Initialiser GPIO som output
    }

    public void Tænd() {
        _erTændt = true;
        // GPIO HIGH
    }

    public void Sluk() {
        _erTændt = false;
        // GPIO LOW
    }

    public void StartBlink() {
        _blinkTimer = new Timer(_ => SkiftLED(), null, 0, 1000); // Blink hvert sekund
    }

    public void StopBlink() {
        _blinkTimer?.Dispose();
        _blinkTimer = null;
        if (_erTændt) Tænd(); else Sluk();
    }

    private void SkiftLED() {
        if (_erTændt) Sluk(); else Tænd();
    }
}