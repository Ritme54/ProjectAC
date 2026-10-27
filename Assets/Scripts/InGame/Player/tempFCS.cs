using UnityEngine;

public class TargetingAndUpperBodyRotation : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;     
    public Transform upperBodyTransform;  
    private Rigidbody2D playerRb;         

    [Header("Fan")]
    public float fanRadius = 5f;          
    [Range(1f, 180f)]
    public float fanAngle = 90f;          

    [Header("Circle")]
    public float circleRadius = 8f;       

    [Header("Rotation Settings")]
    public float rotationSpeed = 15f;     
    public float angleOffset = -90f;      

    [Header("Settings")]
    public LayerMask enemyLayer;          

    private Vector2 lastMoveDirection = Vector2.right; 
    private Transform currentTarget;

    void Start()
    {
        playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb == null)
        {
            Debug.LogWarning("warning!");
        }
    }

    void Update()
    {
        UpdateMovementDirection();

        currentTarget = FindBestTarget();

        if (currentTarget != null)
        {
            RotateUpperBodyTowards(currentTarget.position);
        }
    }

    void UpdateMovementDirection()
    {
        if (playerRb != null)
        {
            Vector2 velocity = playerRb.linearVelocity;
            if (velocity.sqrMagnitude > 0.01f)
            {
                lastMoveDirection = velocity.normalized;
            }
        }
        else
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector2 inputDir = new Vector2(h, v);
            if (inputDir.sqrMagnitude > 0.01f)
            {
                lastMoveDirection = inputDir.normalized;
            }
        }
    }

    Transform FindBestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(playerTransform.position, circleRadius, enemyLayer);

        Transform bestFanTarget = null;
        float minFanDistSqr = Mathf.Infinity;

        Transform bestCircleTarget = null;
        float minCircleDistSqr = Mathf.Infinity;

        Vector2 facingVector = lastMoveDirection;

        foreach (var hit in hits)
        {
            Vector2 dirToEnemy = (Vector2)hit.transform.position - (Vector2)playerTransform.position;
            float distSqr = dirToEnemy.sqrMagnitude;

            if (distSqr < minCircleDistSqr)
            {
                minCircleDistSqr = distSqr;
                bestCircleTarget = hit.transform;
            }

            if (distSqr <= fanRadius * fanRadius)
            {
                float angleToEnemy = Vector2.Angle(facingVector, dirToEnemy);
                if (angleToEnemy <= fanAngle * 0.5f)
                {
                    if (distSqr < minFanDistSqr)
                    {
                        minFanDistSqr = distSqr;
                        bestFanTarget = hit.transform;
                    }
                }
            }
        }

        return bestFanTarget != null ? bestFanTarget : bestCircleTarget;
    }

    void RotateUpperBodyTowards(Vector3 targetPos)
    {
        Vector3 direction = targetPos - upperBodyTransform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angleOffset;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));

        upperBodyTransform.rotation = Quaternion.Slerp(upperBodyTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;

        Vector2 gizmoFacingDir = lastMoveDirection != Vector2.zero ? lastMoveDirection : Vector2.right;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerTransform.position, circleRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerTransform.position, fanRadius);

        float halfAngle = fanAngle * 0.5f;
        Quaternion leftRot = Quaternion.Euler(0, 0, halfAngle);
        Quaternion rightRot = Quaternion.Euler(0, 0, -halfAngle);

        Vector2 leftDir = leftRot * gizmoFacingDir;
        Vector2 rightDir = rightRot * gizmoFacingDir;

        Gizmos.DrawLine(playerTransform.position, (Vector2)playerTransform.position + leftDir * fanRadius);
        Gizmos.DrawLine(playerTransform.position, (Vector2)playerTransform.position + rightDir * fanRadius);
    }
}