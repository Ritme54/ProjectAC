using UnityEngine;

public class Enemy : MonoBehaviour
{
    private MechHull2D mechHull;

    private void Awake()
    {
        mechHull = GetComponent<MechHull2D>();
    }

    private void Update()
    {
        if (mechHull != null && mechHull.IsDead)
        {
            Destroy(gameObject);
        }
    }
}