namespace DataB;

public interface ILEDB
{
    // Interface for LED-driveren
    // Skal kunne tænde, slukke og blinke LED
    void Set(bool on);     // true = LED tændt, false = LED slukket
    void BlinkSlow();      // Får LED til at blinke langsomt (lavt batteri)
}