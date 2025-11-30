namespace DataB;

// Denne klasse sørger for at en epsiode registreres med tidspunkt og varighed i sekunder
public class Episode
{
    public DateTime Tidspunkt { get; set; }         // Hvornår episoden blev registreret
    public int VarighedSekunder { get; set; }       // Hvor længe den varede
}
