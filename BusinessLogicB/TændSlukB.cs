namespace BusinessLogic;
using DataB;
// Alt i denne klasse benyttes ikke - ikke realiseret
// men dette kunne realiseres med LED, batteri og switch senere (hvis vi havde mere tid)
public class TændSlukB
{
private readonly ISwitchB _switch;
private readonly IBatteriB _batteri;
private readonly ILEDB _lysdiode;
private bool _systemErTændt;

// Initialiserer med afhængigheder til hardware-lag
public TændSlukB(ISwitchB switchB, IBatteriB batteri, ILEDB lysdiode)
{
    _switch = switchB;
    _batteri = batteri;
    _lysdiode = lysdiode;
}

// Opdaterer systemstatus og LED-adfærd.
public void OpdaterSystemStatus() 
{
    //Skift systemstatus hvis knappen trykkes
    if (_switch.ErTrykket()) {
        _systemErTændt = !_systemErTændt; // Skift systemstatus
    }

    if (_systemErTændt) {
        int batteriProcent = _batteri.HentBatteriProcent();
        if (batteriProcent < 20) 
        {
            // Lavt batteri → blink
            _lysdiode.StartBlink();
        } 
        else 
        {
            // Batteri OK → konstant lys
            _lysdiode.StopBlink();
            _lysdiode.Tænd();
        }
    } 
    else 
    {
        // System slukket → sluk LED
        _lysdiode.Sluk();
    }
}

//Returnerer om systemet er tændt
public bool ErSystemTændt() => _systemErTændt;

}