using UnityEngine;

public class InGameScene : MonoBehaviour
{
    [SerializeField] private RoomPCG roomPCG;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int stageSeed = 12345;

    private GameObject playerInstance;

    private void Start()
    {
        StartStage(stageSeed);
    }

    public void StartStage(int seed)
    {
        if (roomPCG == null)
        {
            Debug.LogError("[InGameScene] RoomPCG is not assigned.", this);
            return;
        }

        if (!roomPCG.Generate(seed)) return;

        if (playerInstance != null) Destroy(playerInstance);

        Room startRoom = roomPCG.StartRoom;
        if (startRoom == null)
        {
            Debug.LogError("[InGameScene] Generated roomPCG has no start room.", this);
            return;
        }

        Vector3 spawnPosition = startRoom.GetPlayerSpawnPosition();
        if (playerPrefab != null)
        {
            playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("[InGameScene] Player prefab is not assigned; dungeon generated only.", this);
        }
    }
}
