using System.Collections.Generic;
using System.Numerics;
using Content.Server.GameTicking;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Content.Server.Worldgen.Prototypes;
using Content.Server.Worldgen.Components;
using Content.Server._Hullrot.Worldgen.Prototypes;
using Content.Server.Maps;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Test._Hullrot.Worldgen;

/// <summary>
/// Ensures that all maps load correctly on the worlds we use. (Mostly Taypan with or without SRM, etc.)
/// </summary>
[TestFixture]
public sealed class WorldgenTests
{
    private const string DefaultMapName = "Taypan";
    private static readonly string[] Worlds =
    {
        "Taypan"
    };


    [Test, TestCaseSource(nameof(Worlds))]
    public async Task WorldGenerationTest(string worldProtoId)
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            DummyTicker = false,
            Dirty = true,
            InLobby = true,
        });
        var server = pair.Server;

        var entityManager = server.ResolveDependency<IEntityManager>();

        var protoMan = server.ResolveDependency<IPrototypeManager>();
        Assert.That(protoMan.TryIndex<WorldgenConfigPrototype>(worldProtoId, out var worldProto));

        // expected settings for worldgen
        var cfg = server.ResolveDependency<IConfigurationManager>();
        cfg.SetCVar(CCVars.WorldgenEnabled, true);
        cfg.SetCVar(CCVars.WorldgenConfig, worldProtoId);
        cfg.SetCVar(CCVars.GameMap, DefaultMapName);

        // start the round
        var ticker = pair.Server.System<GameTicker>();
        await pair.Server.WaitPost(() => ticker.StartRound());

        // give it the standard ticks to initialize
        await pair.RunTicksSync(10);

        List<(EntityUid uid, WorldControllerComponent controller)> controllers = new();
        EntityUid mapUid = default!;

        // ensure we have one world controller
        var controllerQuery = entityManager.AllEntityQueryEnumerator<WorldControllerComponent>();
        while (controllerQuery.MoveNext(out var uid, out var controller))
        {
            mapUid = uid;
            controllers.Add((uid, controller));
        }

        Assert.That(controllers.Count, Is.EqualTo(1));

        // ensure that it has a placement component with a valid prototype
        Assert.That(entityManager.TryGetComponent<WorldPlacementComponent>(controllers[0].uid, out var placementComponent));
        Assert.That(protoMan.TryIndex<WorldPlacementPrototype>(placementComponent.Prototype, out var placementProto));

        List<Vector2> coords = new();

        // ensure that it's trying to place valid maps
        int expectedStations = 0;
        foreach (var map in placementProto.Maps)
        {
            Assert.That(protoMan.TryIndex<WorldPlacementMapPrototype>(map, out var mapPlacementProto));
            coords.Add(mapPlacementProto.Pos);

            Assert.That(protoMan.TryIndex<GameMapPrototype>(mapPlacementProto.Map, out var mapProto));

            // we expect this map's stations to exist
            expectedStations += mapProto.Stations.Count;
        }

        List<BecomesStationComponent> stations = new();

        var stationQuery = entityManager.AllEntityQueryEnumerator<BecomesStationComponent>();
        while (stationQuery.MoveNext(out var uid, out var station))
        {
            stations.Add(station);
        }

        Assert.That(stations.Count, Is.EqualTo(expectedStations));

        // ensure that each position we're spawning at is part of a grid
        // also it seems like for either the spawn or resolve here we have to
        // wrap it in these awaits to avoid threading errors
        await server.WaitAssertion(() =>
        {
            var xformSys = entityManager.System<SharedTransformSystem>();
            Assert.Multiple(() =>
            {
                foreach (var coord in coords)
                {
                    var banana = entityManager.SpawnEntity("TrashBananaPeel", new EntityCoordinates(mapUid, coord));
                    Assert.That(xformSys.GetGrid(banana) != null);
                }
            });
        });

        await pair.CleanReturnAsync();
    }
}
