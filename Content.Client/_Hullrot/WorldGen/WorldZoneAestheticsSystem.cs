using Content.Shared._Hullrot.Worldgen;
using Content.Shared._Hullrot.Worldgen.Prototypes;
using Robust.Client.Player;

namespace Content.Client._Hullrot.WorldGen;

/// <summary>
/// Client-sided entity system that changes music and parallax based on location within a map, if the map is set up for that.
/// </summary>
public sealed partial class WorldZoneAestheticsSystem : EntitySystem
{
    /// <summary>
    /// The map we were on, the last time we checked.
    /// </summary>
    private int _curMapId = -1;

    /// <summary>
    /// The current map's zone layout, if any.
    /// </summary>
    private WorldZoneAestheticsPrototype[,]? _curZoneMap;

    /// <summary>
    /// Whether we sent a request to the server and haven't received a response yet
    /// </summary>
    private bool _awaitingUpdate = false;

    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_awaitingUpdate)
            return;

        var playerEnt = _playerManager.LocalEntity;

        if (playerEnt == null || !TryComp<TransformComponent>(playerEnt, out var playerXform))
            return;

        if ((int)playerXform.MapID != _curMapId)
        {
            RaiseNetworkEvent(new RequestMapZoneLayoutEvent((int)playerXform.MapID));
            _awaitingUpdate = true;
            return;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<GiveMapZoneLayoutEvent>(OnLayoutReceived);
    }

    private void OnLayoutReceived(GiveMapZoneLayoutEvent args, EntitySessionEventArgs msg)
    {
        _curMapId = args.MapID;
        _awaitingUpdate = false;

        if (args.Layout != null)
        {
            _curZoneMap = new WorldZoneAestheticsPrototype[args.X, args.Y];

            foreach (var coord in args.Layout)
            {
                _curZoneMap[coord.Item2, coord.Item3] = coord.Item1;
            }

        }
        else
        {
            _curZoneMap = null;
        }
    }
}
