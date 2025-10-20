namespace DataB;

public interface ISwitchB
{
    // I vores hard-power-design er softwaren kun "kørende", når der ER strøm,
    // så IsOn() vil i praksis ALTID være true mens programmet kører.
    
        bool IsOn(); // Returnerer true hvis relæ/switch står på "ON"
    
}