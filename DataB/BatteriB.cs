namespace DataB;
using System.Device.Gpio; //skal installeres via NuGet!!!!!!

public class BatteriB
{
    private readonly int _batteriPin = 4; // GPIO4 svarer til fysisk ben 7 på Raspberry Pi

    // Konstruktør: Konfigurerer GPIO og sætter standardværdi for Batteristatus
    public BatteriB()
    {
        var gpio = RPi.Controller;
        gpio.OpenPin(_batteriPin, PinMode.Input);
    }
        
    // Property der gemmer batteriniveauet i procent (0–100)
    public int BatteristatusB { get; private set; } //skal der være både get og set?
        
    // Konstruktør: Initialiserer batteriets status ved oprettelse
    public BatteriB(int batteristatusB)
    {
        BatteristatusB = batteristatusB;
    }

    /* Metode der returnerer true hvis batteriet er opladet (dvs. mere end 0%)
     Denne grænse kan dog rykkes */
    // Returnerer true hvis batteriet er helt tomt (0%)
    public bool erTomt()
    {
        return BatteristatusB == 0;
    }

    // Returnerer true hvis batteriet er lavt (mindre end 20%, men mere end 0)
    public bool erLavt()
    {
        return BatteristatusB > 0 && BatteristatusB < 20;
    }

    // Returnerer true hvis batteriet er tilstrækkeligt opladet (20% eller mere)
    public bool erOpladet()
    {
        return BatteristatusB >= 20;
    }
}
}