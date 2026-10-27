using System.Collections.Generic;
using UnityEngine;

public interface IPenetrable
{
    Collider2D SurfaceCollider { get; }

    // Penetration loss per unit thickness of armor
    float ArmorPerUnit { get; }

    // The section where the bullet's travel segment passes through the inside of the surface
    List<PenetrableSurface2D.Traversal> GetTraversals(Vector2 start, Vector2 end);

    // if(penetrated == true) can penetrate
    void ReceiveImpact(float damage, bool penetrated, Vector2 impactPoint);
}