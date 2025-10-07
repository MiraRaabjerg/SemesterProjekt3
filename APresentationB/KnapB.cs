namespace APresentationB;

public class KnapB
{
    // Denne klasse fungerer som præsentationslaget  (Tænd og Sluk bæltet)
    
    
        // Gemmer systemets aktuelle tilstand (kan fx være "Tændt" eller "Slukket")
        private string tilstandB { get; set; } 
        
        // Controller, som styrer logikken for tænd/sluk-funktionalitet
        private TændSlukB controller;
        
        // Datalagsobjekter til batteri og LED
        BatteriB batteriB = new BatteriB();
        LEDBData ledB = new LEDBData();
        
        // Konstruktør initialiserer controlleren med relevante hardwareklasser
        public KnapB()
        {
            controller = new TændSlukB(batteriB, ledB);
        }

        // Metode der kaldes, når brugeren trykker på knappen for at tænde bæltet
        public void tændB()
        {
            controller.tændB(); // Kalder logikmetoden der tjekker batteristatus og tænder LED

            Console.WriteLine("Systemet er tændt."); //Console kan undlades - debug info
        }

        
        public void slukB()
        {
            controller.slukB(); // Kalder logikmetoden der slukker LED og evt. anden HW
            Console.WriteLine("Systemet er slukket."); //Console kan undlades - debug info
        }
    }
    /*Program kalder Knap, som derefter starter brugsscenariet */




}