using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a fixed room graph once per stage. Grid positions are room-cell coordinates,
/// not tile coordinates. Rooms occupy widthInCells x heightInCells cells.
/// </summary>
public class RoomPCG : MonoBehaviour
{
    [Header("Room definitions")]
    [SerializeField] private List<RoomDefinition> roomDefinitions = new List<RoomDefinition>();
    [SerializeField, Min(1)] private int targetRoomCount = 12;
    [SerializeField, Min(0)] private int seed = 12345;

    [Header("Grid to world")]
    [SerializeField] private Vector2 roomStep = new Vector2(20f, 12f);
    [SerializeField] private Transform roomsRoot;

    public int Seed => seed;
    public IReadOnlyList<Room> SpawnedRooms => spawnedRooms;
    public Room StartRoom { get; private set; }

    private readonly List<Room> spawnedRooms = new List<Room>();
    private readonly Dictionary<Vector2Int, Room> roomAtCell = new Dictionary<Vector2Int, Room>();
    private readonly Dictionary<Room, RoomDefinition> definitionOf = new Dictionary<Room, RoomDefinition>();
    private System.Random rng;

    private static readonly Vector2Int[] Directions =
    {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

    private static DoorDirection ToDoorDirection(int index) => (DoorDirection)index;
    private static DoorDirection Opposite(DoorDirection d) => (DoorDirection)(((int)d + 2) % 4);

    public bool Generate(int stageSeed)
    {
        ClearGenerated();
        seed = stageSeed;
        rng = new System.Random(seed);

        List<RoomDefinition> starts = roomDefinitions.FindAll(r => r != null && r.isStartRoom && r.roomPrefab != null);
        if (starts.Count == 0)
        {
            Debug.LogError("[DungeonGenerator] No start-room definition with a prefab.", this);
            return false;
        }

        RoomDefinition startDef = starts[rng.Next(starts.Count)];
        StartRoom = SpawnRoom(startDef, Vector2Int.zero);
        if (StartRoom == null) return false;

        List<Room> frontier = new List<Room> { StartRoom };
        int safety = Mathf.Max(100, targetRoomCount * 100);
        while (spawnedRooms.Count < targetRoomCount && frontier.Count > 0 && safety-- > 0)
        {
            Room source = frontier[rng.Next(frontier.Count)];
            List<int> openDirections = GetAvailableDirections(source);
            if (openDirections.Count == 0)
            {
                frontier.Remove(source);
                continue;
            }

            Shuffle(openDirections);
            bool placed = false;
            foreach (int directionIndex in openDirections)
            {
                DoorDirection direction = ToDoorDirection(directionIndex);
                Vector2Int offset = Directions[directionIndex];
                Vector2Int sourceCell = GetAnchorCell(source);
                Vector2Int targetCell = sourceCell + offset;

                List<RoomDefinition> candidates = GetCandidates(direction, targetCell);
                Shuffle(candidates);
                foreach (RoomDefinition candidate in candidates)
                {
                    if (!CanPlace(candidate, targetCell)) continue;
                    Room newRoom = SpawnRoom(candidate, targetCell);
                    if (newRoom == null)
                    {
                        continue;
                    }
                    Connect(source, direction, newRoom);
                    frontier.Add(newRoom);
                    placed = true;
                    break;
                }

                if (placed) break;
            }

            if (!placed) frontier.Remove(source);
        }

        // Any remaining doors without a generated neighbor are closed.
        foreach (Room room in spawnedRooms)
        {
            for (int i = 0; i < 4; i++)
            {
                DoorDirection d = ToDoorDirection(i);
                Vector2Int neighborCell = GetAnchorCell(room) + Directions[i];
                bool connected = roomAtCell.TryGetValue(neighborCell, out Room neighbor) && definitionOf[neighbor].HasDoor(Opposite(d));
                room.SetDoorConnected(d, connected);
            }
        }

        if (spawnedRooms.Count < targetRoomCount)
        {
            Debug.LogWarning($"[PCGRoomGenerator] Generated {spawnedRooms.Count}/{targetRoomCount} rooms. " + "Add more compatible room definitions or reduce target count.", this);
        }

        return true;
    }

    public void ClearGenerated()
    {
        foreach (Room room in spawnedRooms)
        {
            if (room != null)
            {
                Destroy(room.gameObject);
            }
        }
        spawnedRooms.Clear();
        roomAtCell.Clear();
        definitionOf.Clear();
        StartRoom = null;
    }

    private Room SpawnRoom(RoomDefinition def, Vector2Int anchorCell)
    {
        if (def == null || def.roomPrefab == null)
        {
            return null;
        }
        Vector3 worldPosition = new Vector3(anchorCell.x * roomStep.x, anchorCell.y * roomStep.y, 0f);
        GameObject instance = Instantiate(def.roomPrefab, worldPosition, Quaternion.identity, roomsRoot != null ? roomsRoot : transform);
        Room room = instance.GetComponent<Room>();
        if (room == null)
        {
            Debug.LogError($"Room prefab '{def.roomPrefab.name}' needs DungeonRoom component.");
            Destroy(instance);
            return null;
        }

        room.RebuildDoorLookup();
        spawnedRooms.Add(room);
        roomAtCell[anchorCell] = room;
        definitionOf[room] = def;

        // All doors start closed. Connected doors are opened by Connect().
        for (int i = 0; i < 4; i++)
        {
            room.SetDoorConnected(ToDoorDirection(i), false);
        }
        
        return room;
    }

    private List<int> GetAvailableDirections(Room room)
    {
        var result = new List<int>();
        RoomDefinition def = definitionOf[room];
        Vector2Int cell = GetAnchorCell(room);
        for (int i = 0; i < 4; i++)
        {
            if (!def.HasDoor(ToDoorDirection(i)))
            {
                continue;
            }
            Vector2Int target = cell + Directions[i];
            if (!roomAtCell.ContainsKey(target))
            {
                result.Add(i);
            }
        }
        return result;
    }

    private List<RoomDefinition> GetCandidates(DoorDirection incoming, Vector2Int targetCell)
    {
        var candidates = roomDefinitions.FindAll(r => r != null && !r.isStartRoom && r.roomPrefab != null && r.weight > 0 && r.HasDoor(Opposite(incoming)));
        // For this first version, every room is a 1x1 cell. Multi-cell support is validated below.
        candidates.RemoveAll(r => r.widthInCells != 1 || r.heightInCells != 1);
        return candidates;
    }

    private bool CanPlace(RoomDefinition candidate, Vector2Int anchorCell)
    {
        if (candidate.widthInCells != 1 || candidate.heightInCells != 1) return false;
        if (roomAtCell.ContainsKey(anchorCell)) return false;

        // Prevent any already placed neighbor from having a door facing a wall.
        for (int i = 0; i < 4; i++)
        {
            Vector2Int neighborCell = anchorCell + Directions[i];
            if (!roomAtCell.TryGetValue(neighborCell, out Room neighbor))
            {
                continue;
            }
            RoomDefinition neighborDef = definitionOf[neighbor];
            bool candidateDoor = candidate.HasDoor(ToDoorDirection(i));
            bool neighborDoor = neighborDef.HasDoor(Opposite(ToDoorDirection(i)));
            if (candidateDoor != neighborDoor)
            {
                return false;
            }
        }
        return true;
    }

    private void Connect(Room a, DoorDirection direction, Room b)
    {
        a.SetDoorConnected(direction, true);
        b.SetDoorConnected(Opposite(direction), true);
    }

    private Vector2Int GetAnchorCell(Room room)
    {
        foreach (var pair in roomAtCell)
        {
            if (pair.Value == room)
            {
                return pair.Key;
            }
        }
        return Vector2Int.zero;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}