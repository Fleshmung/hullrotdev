using Content.Server.Worldgen;
using Content.Server.Worldgen.Components;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

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

        if (!TryComp<WorldControllerComponent>(map, out var controller))
        {
            shell.WriteError("Your mapis not a WorldController.");
            return;
        }

        shell.WriteLine("Your world chunk position is: " + WorldGen.WorldToChunkCoords(_xform.GetWorldPosition((EntityUid)shell.Player.AttachedEntity)).ToString());
    }
}
