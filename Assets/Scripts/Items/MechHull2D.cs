using UnityEngine;

public class MechHull2D : PenetrableSurface2D
{
    [Header("Hull")]
    [SerializeField, Min(0f)]
    private float health = 500f;

    public float Health => health;
    public bool IsDead => health <= 0f;

    public override void ReceiveImpact(float damage, bool penetrated, Vector2 impactPoint)
    {
        if (IsDestroyed || IsDead)
            return;

        if (!penetrated)
            return;

        health = Mathf.Max(0f, health - damage);

        if (health <= 0f)
        {
            Debug.Log($"{name}: Destroyed!");

            // TODO
            // Death Animations or etc Process
        }
    }
}