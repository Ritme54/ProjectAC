using UnityEngine;

public class RoomClearHandler : MonoBehaviour
{
    private Room room;
    private EliminationMode eliminationMode;

    
    private void Awake()
    {
     room = GetComponent<Room>();
        eliminationMode = GetComponent<EliminationMode>();
        

    }

    private void OnEnable()
    {
        eliminationMode.OnCleared += HandleRoomClear;
    }
    private void HandleRoomClear()
    {
        if (room == null)
            return;

        room.isCleared = true;

        foreach (RoomDoor door in room.doors)
        {
            if (door == null)
                continue;

            door.SetConnected(true);
        }

        Debug.Log("[RoomClearHandler] 섬멸전 클리어! 모든 문 개방 완료!");
    }



    private void OnDisable()
    {
        eliminationMode.OnCleared -= HandleRoomClear;
    }




}
