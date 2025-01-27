using Content.Shared._Hullrot.FireControl;
using Content.Shared.Power;

namespace Content.Server._Hullrot.FireControl;

public sealed partial class FireControlSystem : EntitySystem
{
    private void InitializeConsole()
    {
        SubscribeLocalEvent<FireControlConsoleComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<FireControlConsoleComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnPowerChanged(EntityUid uid, FireControlConsoleComponent component, PowerChangedEvent args)
    {
        if (args.Powered)
            TryRegisterConsole(uid, component);
        else
            UnregisterConsole(uid, component);
    }

    private void OnComponentShutdown(EntityUid uid, FireControlConsoleComponent component, ComponentShutdown args)
    {
        UnregisterConsole(uid, component);
    }

    private void UnregisterConsole(EntityUid console, FireControlConsoleComponent? component = null)
    {
        if (!Resolve(console, ref component))
            return;

        component.Connected = false;
        Dirty(console, component);

        var gridServer = TryGetGridServer(console);

        if (gridServer.ServerComponent == null)
            return;

        gridServer.ServerComponent.Consoles.Remove(console);

    }
    private bool TryRegisterConsole(EntityUid console, FireControlConsoleComponent? consoleComponent = null)
    {
        if (!Resolve(console, ref consoleComponent))
            return false;

        var gridServer = TryGetGridServer(console);

        if (gridServer.ServerComponent == null)
            return false;

        if (gridServer.ServerComponent.Consoles.Add(console))
        {
            consoleComponent.Connected = true;
            Dirty(console, consoleComponent);
            return true;
        }
        else
        {
            return false;
        }
    }
}
