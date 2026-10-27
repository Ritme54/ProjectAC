using UnityEngine;

public enum DoorDirection { North = 0, East = 1, South = 2, West = 3 }

[CreateAssetMenu(menuName = "ProjectAC/PCG/Room Definition")]
public class RoomDefinition : ScriptableObject
{
    [Header("Prefab")]
    public GameObject roomPrefab;
    public bool isStartRoom;
    public bool isBossRoom;
    [Min(0)] public int weight = 1;

    [Header("Room grid footprint")]
    [Min(1)] public int widthInCells = 1;
    [Min(1)] public int heightInCells = 1;

    [Header("Available doors")]
    public bool north;
    public bool east;
    public bool south;
    public bool west;

    public bool HasDoor(DoorDirection direction)
    {
        switch (direction)
        {
            case DoorDirection.North: return north;
            case DoorDirection.East: return east;
            case DoorDirection.South: return south;
            case DoorDirection.West: return west;
            default: return false;
        }
    }
}