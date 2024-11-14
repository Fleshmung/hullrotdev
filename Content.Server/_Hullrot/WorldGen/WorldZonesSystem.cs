using Content.Server._Hullrot.Worldgen.Prototypes;
using Content.Server.Worldgen.Prototypes;
using Content.Server.Worldgen.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;


namespace Content.Server._Hullrot.Worldgen;

/// <summary>
/// Handles <see cref="WorldZoneSetupComponent"/> and the placement of zones.
/// </summary>
public sealed class WorldZonesSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ISerializationManager _ser = default!;

    private ISawmill _sawmill = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WorldZoneSetupComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<WorldZoneSetupComponent, WorldChunkAddedEvent>(OnWorldChunkAdded);
    }

    private void OnStartup(EntityUid uid, WorldZoneSetupComponent component, ComponentStartup args)
    {
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
    }

    private void OnWorldChunkAdded(EntityUid uid, WorldZoneSetupComponent component, WorldChunkAddedEvent args)
    {
        if (!_prototypeManager.TryIndex<WorldZoneSetupPrototype>(component.Prototype, out var setupProto))
        {
            _sawmill.Error("Failed to index WorldZoneSetupPrototype " + component.Prototype);
            return;
        }

        // Not initialized? Assume default zone
        if (component.ZoneArray == null)
            ApplyZone(GetZoneProto(setupProto.DefaultZone), args.Chunk);

        ApplyZone(FetchZone(component, setupProto, args.Coords.X, args.Coords.Y), args.Chunk);
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

    private WorldZonePrototype FetchZone(WorldZoneSetupComponent component, WorldZoneSetupPrototype proto, int x, int y)
    {
        return GetZoneProto(proto.DefaultZone);
        // // Any shape rotators in chat?
        // // Let's find (1, 1 first)
        // // Imagine a 2x1
        // // -2, -1, 1, 2
        // // It's the third position, one more than the inradius right?
        // // Well, with a zero-indexed array
        // var baseIndX = proto.Inradius;

        // // now a 1x2
        // // 2
        // // 1
        // // -1
        // // -2
        // var baseIndY = proto.Inradius - 1;

        // // Let's check if we're out of bounds
        // if (baseIndX <)
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
