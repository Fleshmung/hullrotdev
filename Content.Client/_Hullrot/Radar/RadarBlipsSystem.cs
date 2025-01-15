using Content.Shared._Hullrot.Radar;

namespace Content.Client._Hullrot.Radar;

public sealed partial class RadarBlipsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<GiveBlipsEvent>(HandleReceiveBlips);
    }

    private void HandleReceiveBlips(GiveBlipsEvent ev, EntitySessionEventArgs args)
    {

    }

    public void RequestBlips(EntityUid console)
    {
        var netConsole = GetNetEntity(console);

        var ev = new RequestBlipsEvent(netConsole);
        RaiseNetworkEvent(ev);
    }
}
