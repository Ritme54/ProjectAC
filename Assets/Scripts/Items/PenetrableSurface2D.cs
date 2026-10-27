using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public abstract class PenetrableSurface2D : MonoBehaviour, IPenetrable
{
    [Serializable]
    public struct Traversal
    {
        public float EntryT;
        public float ExitT;

        public Vector2 EntryPoint;
        public Vector2 ExitPoint;
        public float Thickness;

        // Does actually penetrate?
        public bool EnteredAtBoundary;
        public bool ExitedAtBoundary;

        public Traversal(float entryT, float exitT, Vector2 start, Vector2 end, bool enteredAtBoundary, bool exitedAtBoundary)
        {
            EntryT = entryT;
            ExitT = exitT;

            EntryPoint = Vector2.Lerp(start, end, entryT);
            ExitPoint = Vector2.Lerp(start, end, exitT);
            Thickness = Vector2.Distance(EntryPoint, ExitPoint);

            EnteredAtBoundary = enteredAtBoundary;
            ExitedAtBoundary = exitedAtBoundary;
        }
    }

    [Header("Armor")]
    [SerializeField, Min(0f)]
    private float armorPerUnit = 10f;

    [Header("Durability")]
    [SerializeField, Min(0f)]
    private float durability = 100f;

    private PolygonCollider2D polygon;

    public Collider2D SurfaceCollider => polygon;
    public float ArmorPerUnit => armorPerUnit;
    public float Durability => durability;
    public bool IsDestroyed { get; private set; }

    protected virtual void Awake()
    {
        polygon = GetComponent<PolygonCollider2D>();
        EnsureArmorLayer();
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        // Auto-configured even when adding components or changing values in the editor.
        EnsureArmorLayer();
    }
#endif

    private void EnsureArmorLayer()
    {
        int armorLayer = LayerMask.NameToLayer("Armor");

        // If the layer is not yet registered in the project, do not assign an arbitrary layer number.
        if (armorLayer == -1)
        {
            return;
        }
        // Not touching other layers
        if (gameObject.layer != armorLayer)
        {
            gameObject.layer = armorLayer;
        }
    }

    public List<Traversal> GetTraversals(Vector2 start, Vector2 end)
    {
        var result = new List<Traversal>();

        if (polygon == null || !polygon.enabled || IsDestroyed)
        {
            return result;
        }

        Vector2 delta = end - start;

        if (delta.sqrMagnitude < 0.000001f)
        {
            return result;
        }
            

        // Collect both endpoints of the movement segment and all polygon boundary intersection points.
        var ts = new List<float> { 0f, 1f };

        for (int pathIndex = 0; pathIndex < polygon.pathCount; pathIndex++)
        {
            Vector2[] path = polygon.GetPath(pathIndex);

            for (int i = 0; i < path.Length; i++)
            {
                Vector2 localA = path[i] + polygon.offset;
                Vector2 localB = path[(i + 1) % path.Length] + polygon.offset;

                Vector2 worldA = transform.TransformPoint(localA);
                Vector2 worldB = transform.TransformPoint(localB);

                if (TryGetIntersectionT(start, end, worldA, worldB, out float t))
                {
                    ts.Add(t);
                }
            }
        }

        ts.Sort();

        // If the same intersection point is registered multiple times, remove the duplicates.
        for (int i = ts.Count - 1; i > 0; i--)
        {
            if (Mathf.Abs(ts[i] - ts[i - 1]) < 0.00001f)
            {
                ts.RemoveAt(i);
            }
        }

        // Each interval between adjacent intersection points is examined individually.
        // Therefore, multiple internal segments within a concave polygon are successfully preserved.
        for (int i = 0; i < ts.Count - 1; i++)
        {
            float entryT = ts[i];
            float exitT = ts[i + 1];

            if (exitT - entryT < 0.00001f)
            {
                continue;
            }
            Vector2 midpoint = Vector2.Lerp(start, end, (entryT + exitT) * 0.5f);

            if (!polygon.OverlapPoint(midpoint))
            {
                continue;
            }
            bool enteredAtBoundary = entryT > 0.00001f || !polygon.OverlapPoint(start);
            bool exitedAtBoundary = exitT < 0.99999f || !polygon.OverlapPoint(end);

            result.Add(new Traversal(entryT, exitT, start, end, enteredAtBoundary, exitedAtBoundary));
        }
        return result;
    }

    private static bool TryGetIntersectionT(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out float t)
    {
        t = 0f;

        Vector2 r = p2 - p1;
        Vector2 s = q2 - q1;

        float denominator = Cross(r, s);

        // Parallel or nearly parallel line segments are excluded here.
        if (Mathf.Abs(denominator) < 0.000001f)
        {
            return false;
        }
        Vector2 qp = q1 - p1;

        float candidateT = Cross(qp, s) / denominator;
        float u = Cross(qp, r) / denominator;

        if (candidateT < 0f || candidateT > 1f ||
            u < 0f || u > 1f)
        {
            return false;
        }

        t = candidateT;
        return true;
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    public virtual void ReceiveImpact(float damage, bool penetrated, Vector2 impactPoint)
    {
        if (IsDestroyed)
        {
            return;
        }
        durability = Mathf.Max(0f, durability - damage);

        if (durability <= 0f)
        {
            OnDestroyed();
        }
    }

    protected virtual void OnDestroyed()
    {
        IsDestroyed = true;

        if (polygon != null)
        {
            polygon.enabled = false;
        }
    }
}