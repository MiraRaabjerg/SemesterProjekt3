namespace DataB;

//Interfacet definerer LED-adfærd og bruges til at styre fysisk LED.
public interface ILEDB
{
    void Tænd();           // Tænder LED
    void Sluk();           // Slukker LED
    void StartBlink();     // Starter blink
    void StopBlink();      // Stopper blink
}