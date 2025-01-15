using Content.Server.Shuttles.Systems;
using Content.Shared._Hullrot.Radar;

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
    }
}
