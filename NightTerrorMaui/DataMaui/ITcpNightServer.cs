using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightTerrorMaui.DataMaui
{
    // Interface for at hente rå måledata via TCP-forbindelse
    // Bruges til at kommunikere med bæltet (RPI)
    public interface ITcpNightServer
    {
        // Henter data fra en TCP-server
        // Returnerer hele svaret som én tekststreng
        Task<string> GetDataAsync(string ip = "raspberrypi.local", int port = 5000);
    }
}

