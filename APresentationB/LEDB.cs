namespace APresentationB;

public class LEDB
{
    // Instanser af business- og datalag
        BatteriB batteriB = new BatteriB();
        LEDBData ledB = new LEDBData();
        
        // Instans af TændSluk-klassen, som håndterer styring af LED via GPIO
        private TændSlukB tændslukB; 

        public LEDB()
        {
            tændSlukB = new TændSlukB(batteriB, ledB); // Opretter en instans af TændSluk klassen
        }

        // LED’ens aktuelle tilstand
        private string tilstandB { get; set; } = "slukket"; // LED starter som slukket
        
        // Timer til blink-funktionalitet
        private System.Timers.Timer myTimer;
        
        /* Metode til at tænde LED’en afhængigt af batteriniveau:
           - Over 20%: LED tændes konstant
           - 1–20%: LED blinker
           - 0%: LED forbliver slukket */
        public void tændB(int BatteristatusB)
        {
            // Hvis batteriet har mere end 20%, så tændes LED’en normalt
            if (BatteristatusB > 20)
            {
                tilstandB = "tændt";
            }

            // Hvis batteriet er mellem 1% og 20%, så blinker LED’en
            else if (BatteristatusB>0) 
            {
                blink();
            }

            // Hvis batteriet er 0%, så forbliver LED'en slukket
            else
            {
                sluk();
            }
        }

        // Metode til at få LED’en til at blinke hvert 0.5 sekund
        public void blinkB()
        {
            // Opretter en timer, der kører hvert 1/2 sekund (500 millisekunder)
            myTimer = new System.Timers.Timer(500);
            
            /* Tilføj en event handler til timeren -> Oprettes for at holde styr på selve timeren
            Dette er et event, som udløses hver gang timeren når sin grænse (interval på 1/2 sek) */
            myTimer.Elapsed += OnTimedEvent; //OnTimedEvent er metoden der kaldes når timeren udløber
            
            // Starter timeren
            myTimer.AutoReset = true; // Hvis timeren skal gentage sig, skal AutoReset være true
            myTimer.Enabled = true; /*Dette betyder at timeren bliver startet og begynder at tælle ned fra 500 millisekunder
                                     og så kører den indtil den bliver stoppet, hvilket kun sker hvis du sætter Enabled = false*/

            Console.WriteLine("Tryk på Enter for at afslutte programmet...");
            Console.ReadLine(); //Holder programmet kørende
        }

        // Hver gang timeren udløses, skifter LED'en tilstand via GPIO
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Timer hændelse: {0:HH:mm:ss.fff}", e.SignalTime);
        }

        // Metode til at slukke LED’en
        public void sluk()
        {
            tilstandB = "slukket"; // Slukker LED via GPIO
            Console.WriteLine("LED er slukket."); 
        }
    }
}