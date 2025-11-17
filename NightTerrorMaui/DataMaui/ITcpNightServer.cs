using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightTerrorMaui.DataMaui
{
    public interface ITcpNightServer
    {
        Task<string> GetDataAsync(string ip = "raspberrypi.local", int port = 5000);
    }
}

