using Unity.VisualScripting;
using UnityEngine;

public class Socket : MonoBehaviour
{
    Transform socket;

    private void Start()
    {
        socket = GetComponent<Transform>();
    }

    private void Equipment(GameObject _weapon)//Drag weapon, and equip
    {
        if (socket.childCount > 0)
        {
            Debug.Log("Already equip a weapon.");
            return;
        }

        GameObject curWeapon = Instantiate(_weapon, socket);

        curWeapon.transform.localPosition = Vector2.zero;
        curWeapon.transform.localRotation = Quaternion.identity;
        curWeapon.transform.localScale = Vector2.one;
    }
}