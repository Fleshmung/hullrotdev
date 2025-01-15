using System.Numerics;
using Content.Server.Shuttles.Systems;
using Content.Shared._Hullrot.Radar;
using Content.Shared.Shuttles.Components;

namespace Content.Server._Hullrot.Radar;

/// <summary>
/// I debated making <see cref="RadarConsoleSystem"/> partial
/// but ended up doing this instead to mnimize conflicts. This system
/// handles radar blips -- which, due to both the limitations of PVS range
/// and against giving the client too much info must be server side.
/// </summary>
public sealed partial class RadarBlipSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RequestBlipsEvent>(OnBlipsRequested);
    }

    private void OnBlipsRequested(RequestBlipsEvent ev, EntitySessionEventArgs args)
    {
        if (!TryGetEntity(ev.Radar, out var radarUid))
            return;

        if (!TryComp<RadarConsoleComponent>(radarUid, out var radar))
            return;

        var blips = new List<(Vector2, float, Color)>();

        var giveEv = new GiveBlipsEvent(blips);
        RaiseNetworkEvent(giveEv, args.SenderSession);
    }
}
