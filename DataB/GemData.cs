namespace DataB;

public class GemData
{
    private readonly List<Episode> _episoder = new();

    public void GemEpisode(DateTime tidspunkt, int varighedSekunder)
    {
        var episode = new Episode
        {
            Tidspunkt = tidspunkt,
            VarighedSekunder = varighedSekunder
        };

        _episoder.Add(episode);
    }

    public List<Episode> HentAlleEpisoder() => _episoder;
}

public class Episode
{
    public DateTime Tidspunkt { get; set; }
    public int VarighedSekunder { get; set; }
}
