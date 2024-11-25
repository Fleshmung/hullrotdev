using Content.Server.Worldgen.Components;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Shared._Hullrot.Worldgen;
using Content.Server._Hullrot.Worldgen.Prototypes;

namespace Content.Server._Hullrot.Worldgen;

public sealed partial class WorldZonesSystem
{
    private void InitializeCommands()
    {
        _console.RegisterCommand("worldchunk", "Get the chunk coordinates of your current position", "No arguments", GetWorldChunk);
    }

    [AdminCommand(AdminFlags.Debug)]
    private void GetWorldChunk(IConsoleShell shell, string argstr, string[] args)
    {
        if (shell.Player?.AttachedEntity == null)
            return;

        if (!_map.TryGetMap(Transform((EntityUid)shell.Player.AttachedEntity).MapID, out var map))
        {
            shell.WriteError("You are not on a map.");
            return;
        }

        if (!HasComp<WorldControllerComponent>(map))
        {
            shell.WriteError("Your map is not a WorldController.");
            return;
        }

        var chunk = HullrotWorldGen.WorldToChunkCoords(_xform.GetWorldPosition((EntityUid)shell.Player.AttachedEntity));

        shell.WriteLine("Your world chunk position is: " + chunk.ToString());

        if (!TryComp<WorldZoneSetupComponent>(map, out var setup))
            return;

        if (setup.Prototype == null || !_prototypeManager.TryIndex<WorldZoneSetupPrototype>(setup.Prototype, out var proto))
            return;

        var curZone = FetchZone(setup, GetZoneProto(proto.OobZone ?? proto.DefaultZone), chunk);

        shell.WriteLine("Current zone proto is " + curZone.ID);
    }
}
