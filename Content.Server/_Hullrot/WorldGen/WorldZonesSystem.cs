using Content.Server._Hullrot.Worldgen.Prototypes;
using Content.Server.Worldgen.Prototypes;
using Content.Server.Worldgen.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Console;
using System.Numerics;
using Content.Shared._Hullrot.Worldgen;
using Content.Shared._Hullrot.Worldgen.Prototypes;

namespace Content.Server._Hullrot.Worldgen;

/// <summary>
/// Handles <see cref="WorldZoneSetupComponent"/> and the placement of zones.
/// </summary>
public sealed partial class WorldZonesSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConsoleHost _console = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ISerializationManager _ser = default!;

    private ISawmill _sawmill = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WorldZoneSetupComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<WorldZoneSetupComponent, WorldChunkAddedEvent>(OnWorldChunkAdded);

        SubscribeNetworkEvent<RequestMapZoneLayoutEvent>(OnRequestLayout);

        InitializeCommands();
    }

    private void OnStartup(EntityUid uid, WorldZoneSetupComponent component, ComponentStartup args)
    {
        // Since datafield is required this will probably only be true with e.g. tests adding all comps
        if (component.Prototype == null)
            return;

        if (!_prototypeManager.TryIndex<WorldZoneSetupPrototype>(component.Prototype, out var setupProto))
        {
            _sawmill.Error("Failed to index WorldZoneSetupPrototype " + component.Prototype);
            return;
        }

        if (setupProto.Inradius < 1)
        {
            _sawmill.Error("Trying to initialize empty / negative area zone map");
            return;
        }

        // Initialize the zone array
        component.ZoneArray = new WorldZonePrototype[setupProto.Inradius * 2, setupProto.Inradius * 2];

        var defaultZone = GetZoneProto(setupProto.DefaultZone);

        // Initialize default values
        for (int i = 0; i < component.ZoneArray.GetLength(0); i++)
            for (int k = 0; k < component.ZoneArray.GetLength(1); k++)
                component.ZoneArray[i, k] = defaultZone;

        // overwrite with zone protos
        foreach (var zone in setupProto.Zones)
        {
            if (!_prototypeManager.TryIndex<WorldZonePrototype>(zone, out var zoneProto))
            {
                _sawmill.Error("Failed to index WorldZonePrototype " + zone);
                continue;
            }

            foreach (var tile in zoneProto.Tiles)
            {
                if (!ChunkToArrayCoords(component.ZoneArray, tile, out var coords))
                {
                    _sawmill.Warning("Chunk coord " + tile + "in zone prototype " + zoneProto.ID + " is out of bounds");
                    continue;
                }

                component.ZoneArray[coords.X, coords.Y] = zoneProto;
            }
        }
    }

    private void OnWorldChunkAdded(EntityUid uid, WorldZoneSetupComponent component, WorldChunkAddedEvent args)
    {
        if (component.Prototype == null || !_prototypeManager.TryIndex<WorldZoneSetupPrototype>(component.Prototype, out var setupProto))
        {
            _sawmill.Error("Failed to index WorldZoneSetupPrototype " + component.Prototype);
            return;
        }

        // Not initialized? Assume default zone
        if (component.ZoneArray == null)
            ApplyZone(GetZoneProto(setupProto.DefaultZone), args.Chunk);

        ApplyZone(FetchZone(component, GetZoneProto(setupProto.OobZone ?? setupProto.DefaultZone), args.Coords), args.Chunk);
    }

    private void OnRequestLayout(RequestMapZoneLayoutEvent ev, EntitySessionEventArgs args)
    {
        if (!_map.TryGetMap(new Robust.Shared.Map.MapId(ev.MapID), out var map))
        {
            _sawmill.Error("Client " + args.SenderSession.Name + " requested invalid map id " + ev.MapID);
            RaiseNetworkEvent(new GiveMapZoneLayoutEvent(ev.MapID, 0, 0, null), args.SenderSession);
            return;
        }

        if (!TryComp<WorldZoneSetupComponent>(map, out var setup) || setup.ZoneArray == null)
        {
            RaiseNetworkEvent(new GiveMapZoneLayoutEvent(ev.MapID, 0, 0, null), args.SenderSession);
            return;
        }

        var serverArray = setup.ZoneArray;

        // Can't serialize so going joker mode
        var clientArray = new List<(WorldZoneAestheticsPrototype, int, int)>();

        for (int i = 0; i < serverArray.GetLength(0); i++)
        {
            for (int k = 0; k < serverArray.GetLength(1); k++)
            {
                var serverCell = serverArray[i, k];

                if (!_prototypeManager.TryIndex<WorldZoneAestheticsPrototype>(serverCell.Aesthetics, out var aesth))
                {
                    _sawmill.Error("Trying to index invalid aesthetics prototype " + serverCell.Aesthetics);
                    continue;
                }

                clientArray.Add((aesth, i, k));
            }
        }
        RaiseNetworkEvent(new GiveMapZoneLayoutEvent(ev.MapID, serverArray.GetLength(0), serverArray.GetLength(1), clientArray), args.SenderSession);
    }

    private void ApplyZone(WorldZonePrototype zone, EntityUid chunk)
    {
        if (!_prototypeManager.TryIndex<BiomePrototype>(zone.Biome, out var biome))
        {
            _sawmill.Error("Failed to index BiomePrototype " + zone);
            return;
        }

        biome.Apply(chunk, _ser, EntityManager);
    }

    private WorldZonePrototype FetchZone(WorldZoneSetupComponent component, WorldZonePrototype fallback, Vector2 chunkCoords)
    {
        return FetchZone(component, fallback, new Vector2i((int)Math.Floor(chunkCoords.X), (int)Math.Floor(chunkCoords.Y)));
    }

    private WorldZonePrototype FetchZone(WorldZoneSetupComponent component, WorldZonePrototype fallback, Vector2i chunkCoords)
    {
        if (component.ZoneArray != null && ChunkToArrayCoords(component.ZoneArray, chunkCoords, out var arrayCoords))
        {
            return component.ZoneArray[arrayCoords.X, arrayCoords.Y];
        }

        return fallback;
    }

    private bool ChunkToArrayCoords(WorldZonePrototype[,] array, Vector2i coords, out Vector2i arrayCoords)
    {
        arrayCoords = Vector2i.Zero;

        // These map to 0, 0 in chunk coords
        var baseIndX = array.GetLength(0) / 2;
        var baseIndY = array.GetLength(1) / 2 - 1;

        var indX = baseIndX + coords.X;
        var indY = baseIndY - coords.Y;

        // Check if we're out of bounds
        if (indX < 0 || indX > array.GetLength(0) - 1
        || indY < 0 || indY > array.GetLength(1) - 1)
            return false;

        arrayCoords = new Vector2i(indX, indY);
        return true;
    }
    private WorldZonePrototype GetZoneProto(string proto)
    {
        if (!_prototypeManager.TryIndex<WorldZonePrototype>(proto, out var zonePrototype))
        {
            _sawmill.Error("Failed to load WorldZonePrototype " + proto);
            return default!;
        }

        return zonePrototype;
    }
}
