/// Copyright Rane (elijahrane@gmail.com) 2025
/// All rights reserved.

namespace Content.Server._Hullrot.FireControl;

/// My fire control system replaces the functionality of point cannons.
/// I'll keep my notes here on how it works.
/// 1) Ships have a central fire control server that keeps track of the available remote weapons (turrets, drones, etc)
/// 2) The server leases control of these weapons to targeting consoles, etc, allowing multiple targeting consoles on a ship
/// 3) Targeting consoles generally have RTS style selection controls in addition to tags
/// 4) Control groups from that are not diegetic and lets players personalize them


[RegisterComponent]
public sealed partial class FireControlServerComponent : Component
{
    [ViewVariables]
    public EntityUid? ConnectedGrid = null;
}
