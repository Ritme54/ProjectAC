
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room local points")]
    public Transform playerSpawnPoint;
    public RoomDoor[] doors;

    [Header("Optional gameplay hooks")]
    public bool lockDoorsOnEnter;
    public bool isCleared;

    private readonly Dictionary<DoorDirection, RoomDoor> doorByDirection = new Dictionary<DoorDirection, RoomDoor>();

    private void Awake()
    {
        RebuildDoorLookup();
    }

    public void RebuildDoorLookup()
    {
        doorByDirection.Clear();
        if (doors == null)
        {
            return;
        }
        foreach (RoomDoor door in doors)
        {
            if (door == null)
            {
                continue;
            }
            doorByDirection[door.direction] = door;
        }
    }

    public RoomDoor GetDoor(DoorDirection direction)
    {
        if (doorByDirection.Count == 0)
        {
            RebuildDoorLookup();
        }
        doorByDirection.TryGetValue(direction, out RoomDoor result);
        return result;
    }

    public void SetDoorConnected(DoorDirection direction, bool connected)
    {
        RoomDoor door = GetDoor(direction);
        if (door != null)
        {
            door.SetConnected(connected);
        }
    }

    public Vector3 GetPlayerSpawnPosition()
    {
        return playerSpawnPoint != null ? playerSpawnPoint.position : transform.position;
    }
}

