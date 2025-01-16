/// Copyright Rane (elijahrane@gmail.com) 2025
/// All rights reserved.

using Content.Shared.Power;

namespace Content.Server._Hullrot.FireControl;

public sealed class FireControlSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    /// My fire control system replaces the functionality of point cannons.
    /// I'll keep my notes here on how it works.
    /// 1) Ships have a central fire control server that keeps track of the available remote weapons (turrets, drones, etc)
    /// 2) The server leases control of these weapons to targeting consoles, etc, allowing multiple targeting consoles on a ship
    /// 3) Targeting consoles generally have RTS style selection controls in addition to tags
    /// 4) Control groups from that are not diegetic and lets players personalize them

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FireControlServerComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<FireControlServerComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<FireControllableComponent, PowerChangedEvent>(OnControllablePowerChanged);
        SubscribeLocalEvent<FireControllableComponent, ComponentShutdown>(OnControllableShutdown);
    }

    private void OnPowerChanged(EntityUid uid, FireControlServerComponent component, PowerChangedEvent args)
    {
        if (args.Powered)
            TryConnect(uid, component);
        else
            Disconnect(uid, component);
    }

    private void OnShutdown(EntityUid uid, FireControlServerComponent component, ComponentShutdown args)
    {
        Disconnect(uid, component);
    }

    private void OnControllablePowerChanged(EntityUid uid, FireControllableComponent component, PowerChangedEvent args)
    {
        if (args.Powered)
            TryRegister(uid, component);
        else
            Unregister(uid, component);
    }

    private void OnControllableShutdown(EntityUid uid, FireControllableComponent component, ComponentShutdown args)
    {
        Unregister(uid, component);
    }

    private void Disconnect(EntityUid server, FireControlServerComponent? component = null)
    {
        if (!Resolve(server, ref component))
            return;

        if (!Exists(component.ConnectedGrid) || !TryComp<FireControlGridComponent>(component.ConnectedGrid, out var controlGrid))
            return;

        if (controlGrid.ControllingServer == server)
        {
            controlGrid.ControllingServer = null;
            RemComp<FireControlGridComponent>((EntityUid)component.ConnectedGrid);
        }
    }

    private bool TryConnect(EntityUid server, FireControlServerComponent? component = null)
    {
        if (!Resolve(server, ref component))
            return false;

        var grid = _xform.GetGrid(server);

        if (grid == null)
            return false;

        var controlGrid = EnsureComp<FireControlGridComponent>((EntityUid)grid);

        if (controlGrid.ControllingServer != null)
            return false;

        controlGrid.ControllingServer = server;
        component.ConnectedGrid = grid;
        return true;
    }

    private void Unregister(EntityUid controllable, FireControllableComponent? component = null)
    {
        if (!Resolve(controllable, ref component))
            return;

        if (component.ControllingServer == null || !TryComp<FireControlServerComponent>(component.ControllingServer, out var controlComp))
            return;

        controlComp.Controlled.Remove(controllable);
    }

    private bool TryRegister(EntityUid controllable, FireControllableComponent? component = null)
    {
        if (!Resolve(controllable, ref component))
            return false;

        var grid = _xform.GetGrid(controllable);

        if (grid == null)
            return false;

        if (!TryComp<FireControlGridComponent>(grid, out var controlGrid))
            return false;

        if (controlGrid.ControllingServer == null || !TryComp<FireControlServerComponent>(controlGrid.ControllingServer, out var server))
            return false;

        return server.Controlled.Add(controllable);
    }
}
