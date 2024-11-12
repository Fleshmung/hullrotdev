using System.Collections.Generic;
using System.IO;
using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Spawners.Components;
using Content.Server.Station.Components;
using Content.Shared.CCVar;
using Content.Shared.Roles;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Station.Components;
using FastAccessors;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;
using Robust.Shared;
using Content.Server.Worldgen.Prototypes;
using Content.Server.Worldgen.Components;
using Content.Server._Hullrot.Worldgen.Prototypes;
using BenchmarkDotNet.Disassemblers;


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

        // ensure we have one world controller
        var controllerQuery = entityManager.AllEntityQueryEnumerator<WorldControllerComponent>();
        while (controllerQuery.MoveNext(out var uid, out var controller))
        {
            controllers.Add((uid, controller));
        }

        Assert.That(controllers.Count, Is.EqualTo(1));

        // ensure that it has a placement component with a valid prototype
        Assert.That(entityManager.TryGetComponent<WorldPlacementComponent>(controllers[0].uid, out var placementComponent));
        Assert.That(protoMan.TryIndex<WorldPlacementPrototype>(placementComponent.Prototype, out var placementProto));

        // ensure that it's trying to place valid maps
        int expectedStations = 0;
        foreach (var map in placementProto.Maps)
        {
            Assert.That(protoMan.TryIndex<GameMapPrototype>(map, out var mapProto));

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

        await pair.CleanReturnAsync();
    }
}
