using Unity.VisualScripting;
using UnityEngine;

public class Socket : MonoBehaviour
{
    Transform socket;

    private void Equipment(GameObject _weapon)//Drag weapon, and equip
    {
        GameObject curWeapon = Instantiate(_weapon, socket.transform);

        curWeapon.transform.localPosition = Vector2.zero;
        curWeapon.transform.localRotation = Quaternion.identity;
        curWeapon.transform.localScale = Vector2.one;
    }
}
