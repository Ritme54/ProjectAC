using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    public DoorDirection direction;
    [Tooltip("Optional objects to disable when this door is connected/open.")]
    public GameObject[] closedDoorObjects;
    public Collider2D[] blockingColliders;

    public void SetConnected(bool connected)
    {
        bool closed = !connected;

        if (closedDoorObjects != null)
        {
            foreach (var obj in closedDoorObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(closed);
                }
            }
        }

        if (blockingColliders != null)
        {
            foreach (var col in blockingColliders)
            {
                if (col != null)
                {
                    col.enabled = closed;
                }
            }
        }
    }
}