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
        Logger.Error("Received blips. Count: " + ev.Blips.Count);
        foreach (var blip in ev.Blips)
        {
            Logger.Error("Pos: " + blip.Item1);
            Logger.Error("Scale: " + blip.Item2);
            Logger.Error("Color: " + blip.Item3);
        }
    }

    public void RequestBlips(EntityUid console)
    {
        var netConsole = GetNetEntity(console);

        var ev = new RequestBlipsEvent(netConsole);
        RaiseNetworkEvent(ev);
    }
}
