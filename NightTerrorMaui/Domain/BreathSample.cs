using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightTerrorMaui.Domain
{
    public class BreathSample
    {
        public DateTime Time { get; set; }
        public double Frequency { get; set; }  // fx vejrtrækninger pr. minut

        public BreathSample() { }

        public BreathSample(DateTime time, double frequency)
        {
            Time = time;
            Frequency = frequency;
        }
    }
}
