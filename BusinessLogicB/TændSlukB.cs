namespace BusinessLogic;
using DataB;

public class TændSlukB
{
private readonly ISwitchB _switch;
private readonly IBatteriB _batteri;
private readonly ILEDB _lysdiode;
private bool _systemErTændt;

public TændSlukB(ISwitchB switchB, IBatteriB batteri, ILEDB lysdiode)
{
    _switch = switchB;
    _batteri = batteri;
    _lysdiode = lysdiode;
}

// Kaldes fx hvert sekund i main loop
public void OpdaterSystemStatus() 
{
    if (_switch.ErTrykket()) {
        _systemErTændt = !_systemErTændt; // Skift systemstatus
    }

    if (_systemErTændt) {
        int batteriProcent = _batteri.HentBatteriProcent();
        if (batteriProcent < 20) {
            _lysdiode.StartBlink();
        } else {
            _lysdiode.StopBlink();
            _lysdiode.Tænd();
        }
    } else {
        _lysdiode.Sluk();
    }
}

public bool ErSystemTændt() => _systemErTændt;

}