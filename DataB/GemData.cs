namespace DataB
{
    public class GemData
    {
        // Alle m�linger (fx glattet respirationssignal)
        private readonly List<double> _samples = new();

        // Alle episoder
        private readonly List<Episode> _episoder = new();

        // Gem �n m�ling
        public void GemMåling(double værdi)
        {
            _samples.Add(værdi);
        }

        // Hent alle m�linger (til Netv�rk/TCP)
        public IReadOnlyList<double> HentAlleMålinger()
        {
            return _samples;
        }

        // Din eksisterende episode-kode:
        public void GemEpisode(DateTime tidspunkt, int varighedSekunder)
        {
            var episode = new Episode
            {
                Tidspunkt = tidspunkt,
                VarighedSekunder = varighedSekunder
            };

            _episoder.Add(episode);
        }

        public IReadOnlyList<Episode> HentAlleEpisoder()
        {
            return _episoder;
        }
    }
}
