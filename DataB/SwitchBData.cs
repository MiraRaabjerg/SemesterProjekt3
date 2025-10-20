namespace DataB;

public class SwitchBData : ISwitchB
{
    private readonly int _gpioPin;

    public SwitchBData(int gpioPin)
    {
        _gpioPin = gpioPin;
        // Initialiser GPIO som input
    }
        public bool ErTrykket()
        { // Læs GPIO pin
            // returner true hvis HIGH, ellers false
            return false; // placeholder
        }
    
}



    
