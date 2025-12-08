using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightTerrorMaui.Domain
{
    // BreathSample repræsenterer én måling af vejrtrækningsfrekvens på et bestemt tidspunkt
    public class BreathSample
    {
        // Tidspunkt for målingen (fx klokken 01:23:45)
        public DateTime Time { get; set; }
        
        // Vejrtrækningsfrekvens ved dette tidspunkt (fx antal pr. minut)
        public double Frequency { get; set; }  // fx vejrtrækninger pr. minut

        // tom ctor - bruges ikke
        public BreathSample() { }
        
        //Ctor med tid og frekvens
        public BreathSample(DateTime time, double frequency)
        {
            Time = time;
            Frequency = frequency;
        }
    }
}
