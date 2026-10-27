using System.Collections.Generic;
using UnityEngine;

public class Projectile2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float speed = 30f;
    [SerializeField]
    public Vector2 direction = Vector2.zero;

    [SerializeField, Min(0.001f)]
    private float maxLifetime = 5f;

    [Header("Combat")]
    [SerializeField, Min(0f)]
    private float penetrationPower = 100f;

    [SerializeField, Min(0f)]
    private float impactDamage = 25f;

    [Header("Collision Query")]
    [SerializeField, Min(0.001f)]
    private float queryPadding = 0.05f;

    private int wallLayerMask;
    private int armorLayerMask;
    private float elapsedTime;
    private bool spent;

    // Prevent a single projectile from applying duplicate hit damage to the same surface.
    // Penetration cost is calculated independently for each internal segment.
    private readonly HashSet<IPenetrable> damagedSurfaces = new();

    private struct HitTraversal
    {
        public IPenetrable Surface;
        public PenetrableSurface2D.Traversal Traversal;

        public HitTraversal(IPenetrable surface, PenetrableSurface2D.Traversal traversal)
        {
            Surface = surface;
            Traversal = traversal;
        }
    }

    private void Awake()
    {
        int armorLayer = LayerMask.NameToLayer("Armor");
        int wallLayer = LayerMask.NameToLayer("Wall");

        if (armorLayer < 0)
        {
            Debug.LogError("Projectile2D: 'Armor' Layer is not valid. " + "Create one in Project Settings > Tags and Layers.", this);

            armorLayerMask = 0;
            return;
        }

        if (armorLayer == -1)
        {
            Debug.LogError("Projectile2D: 'Armor' Layer is not valid. Create one in Project Settings > Tags and Layers.", this);
        }
        else
        {
            armorLayerMask = 1 << armorLayer;
        }

        if (wallLayer == -1)
        {
            Debug.LogError("Projectile2D: 'Wall' Layer is not valid. Create one in Project Settings > Tags and Layers.", this);
        }
        else
        {
            wallLayerMask = 1 << wallLayer;
        }
    }

    private void Update()
    {
        if (spent)
        {
            return;
        }
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (armorLayerMask == 0)
        {
            return;
        }

        Vector2 start = transform.position;
        direction = transform.up;
        Vector2 end = start + direction * speed * Time.deltaTime;

        ProcessSegment(start, end);

        if (!spent)
        {
            transform.position = end;
        }
    }

    private void ProcessSegment(Vector2 start, Vector2 end)
    {
        // 1. Checks for the first wall along the movement path on the Wall layer.
        RaycastHit2D wallHit = Physics2D.Linecast(start, end, wallLayerMask);

        bool hitWall = wallHit.collider != null;

        // If a wall is hit, restrict the armor check only up to the front of that wall.
        Vector2 armorEnd = hitWall ? wallHit.point - direction * 0.001f : end;

        // 2. Calculate the AABB for screening armor candidates.
        Vector2 padding = Vector2.one * queryPadding;
        Vector2 min = Vector2.Min(start, armorEnd) - padding;
        Vector2 max = Vector2.Max(start, armorEnd) + padding;

        Collider2D[] candidates = Physics2D.OverlapAreaAll(min, max, armorLayerMask);

        var hits = new List<HitTraversal>();
        var seenSurfaces = new HashSet<IPenetrable>();

        foreach (Collider2D candidate in candidates)
        {
            // Retrieve the penetration surface from the Armor layer's collider.
            IPenetrable surface = candidate.GetComponent(typeof(IPenetrable)) as IPenetrable;

            if (surface == null)
            {
                surface = candidate.GetComponentInParent(typeof(IPenetrable)) as IPenetrable;
            }

            if (surface == null)
            {
                continue;
            }

            Collider2D surfaceCollider = surface.SurfaceCollider;

            if (surfaceCollider == null || !surfaceCollider.enabled || !surfaceCollider.gameObject.activeInHierarchy)
            {
                continue;
            }

            // Examine each surface only once.
            if (!seenSurfaces.Add(surface))
                continue;

            // Calculate internal armor segments only up to the point before hitting a wall.
            List<PenetrableSurface2D.Traversal> traversals = surface.GetTraversals(start, armorEnd);

            foreach (var traversal in traversals)
            {
                hits.Add(new HitTraversal(surface, traversal));
            }
        }

        // 3. Process armor starting from the first one encountered along the bullet's trajectory.
        hits.Sort((a, b) => a.Traversal.EntryT.CompareTo(b.Traversal.EntryT));

        foreach (HitTraversal hit in hits)
        {
            if (spent)
            {
                return;
            }

            IPenetrable surface = hit.Surface;
            var traversal = hit.Traversal;

            Collider2D collider = surface.SurfaceCollider;

            if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy)
            {
                continue;
            }

            float requiredPenetration = surface.ArmorPerUnit * traversal.Thickness;

            // 4. If penetration power is insufficient, stop inside the armor.
            if (penetrationPower < requiredPenetration)
            {
                float distanceInside = surface.ArmorPerUnit > 0f ? penetrationPower / surface.ArmorPerUnit : 0f;

                Vector2 insideDirection = (traversal.ExitPoint - traversal.EntryPoint).normalized;

                Vector2 impactPoint = traversal.EntryPoint + insideDirection * distanceInside;

                transform.position = impactPoint;

                ApplyImpactOnce(surface, false, impactPoint);

                spent = true;
                Destroy(gameObject);
                return;
            }

            // 5. Deduct the penetration cost required to traverse the internal armor segment.
            penetrationPower -= requiredPenetration;

            // Record as a successful penetration only if it clears the actual exit.
            if (traversal.ExitedAtBoundary)
            {
                ApplyImpactOnce(surface, true, traversal.ExitPoint);
            }

            // If penetration power is exhausted, stop at the end of the current segment.
            if (penetrationPower <= 0f)
            {
                transform.position = traversal.ExitPoint;

                spent = true;
                Destroy(gameObject);
                return;
            }
        }

        // 6. If the projectile survives after processing all armor, stop at the wall.
        if (hitWall)
        {
            transform.position = wallHit.point;

            spent = true;
            Destroy(gameObject);
        }
    }
    private void ApplyImpactOnce(IPenetrable surface, bool penetrated, Vector2 impactPoint)
    {
        if (!damagedSurfaces.Add(surface))
            return;

        surface.ReceiveImpact(impactDamage, penetrated, impactPoint);
    }
}