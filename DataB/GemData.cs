namespace DataB
{
    // denne klasse gemmer vores målinger, så de senere kan bruges til analyse eller via netværk
    // denne klasse er ikke realiseret, så den kan ikke gemme data
    public class GemData
    {
        // Liste med alle målinger
        private readonly List<double> _samples = new();

        // Liste med alle episoder
        private readonly List<Episode> _episoder = new();

        // Gem en måling i listen
        public void GemMåling(double værdi)
        {
            _samples.Add(værdi);
        }

        // Hent alle målinger (til Netværk/TCP)
        public IReadOnlyList<double> HentAlleMålinger()
        {
            return _samples;
        }

        // Opretter og gemmer en ny episode med tidspunkt og varighed
        public void GemEpisode(DateTime tidspunkt, int varighedSekunder)
        {
            var episode = new Episode
            {
                Tidspunkt = tidspunkt,
                VarighedSekunder = varighedSekunder
            };

            _episoder.Add(episode);
        }

       // Returnerer alle gemte episoder som en ReadOnly liste, som kan bruges til analyse eller netværk
        public IReadOnlyList<Episode> HentAlleEpisoder()
        {
            return _episoder;
        }
    }
}
