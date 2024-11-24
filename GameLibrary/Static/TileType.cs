namespace GameLibrary
{
    public enum TileType
    {
        Disabled, // Unreachable tile
        Floor, // A walkable tile
        HomeDoor, // Only ghosts can pass this tile
        HomeInterior, // Ghost only home
        Intersection, // Ghosts make a decision when one tile from an intersection
        Restricted, // Ghosts can't turn up when restricted
        Teleport, // Teleport the player/ghost if they're on this tile
        Tunnel, // Ghosts slow down in the tunnel
        Wall // An impassable tile
    }
}
