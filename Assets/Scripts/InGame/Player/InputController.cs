using UnityEngine;

public enum MoveType
{
    None,
    Leg,
    Fly
}

public class InputController : MonoBehaviour
{
    public MoveType moveType;

    Rigidbody2D playerRigid;
    Vector2 inputVec;

    [SerializeField]
    private float limitWeightMaxSpeed;
    [SerializeField]
    private float normalMaxSpeed;
    [SerializeField]
    private float stopFric;

    private void Start()
    {
        playerRigid = GetComponent<Rigidbody2D>();
        limitWeightMaxSpeed = PlayerInfo.Instance.maxSpeed / 2;
        normalMaxSpeed = PlayerInfo.Instance.maxSpeed;
    }

    private void Update()
    {
        inputVec = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    private void FixedUpdate()
    {
        HowtoMove(moveType);
    }

    private void HowtoMove(MoveType _moveType)
    {
        LimitSpeed();

        if (_moveType == MoveType.Leg)
        {
            LegMove();
        }
        else if (_moveType == MoveType.Fly)
        {
            FlyMove();
        }
    }

    private void LegMove()
    {
        if (inputVec != Vector2.zero)
        {
            float curSpeed = Mathf.Min(PlayerInfo.Instance.axel / 4f, PlayerInfo.Instance.maxSpeed / 4f);
            Vector2 force = playerRigid.position + inputVec * curSpeed * Time.fixedDeltaTime;
            playerRigid.MovePosition(force);
        }
    }


    private void FlyMove()
    {
        if (inputVec != Vector2.zero)
        {
            Vector2 velocity = playerRigid.linearVelocity;

            if (velocity.magnitude > 0f)
            {
                float dot = Vector2.Dot(inputVec, velocity.normalized);
                if (dot < 0)
                {
                    playerRigid.linearVelocity *= -dot;
                }
            }
            Vector2 force = inputVec * PlayerInfo.Instance.axel;
            playerRigid.AddForce(force, ForceMode2D.Force);
        }
        else
        {
            playerRigid.linearVelocity = new Vector2(playerRigid.linearVelocity.x * stopFric, playerRigid.linearVelocity.y * stopFric);
        }
    }

    private void LimitSpeed()
    {
        PlayerInfo.Instance.maxSpeed = PlayerInfo.Instance.curWeight >= PlayerInfo.Instance.limitWeight ? limitWeightMaxSpeed : normalMaxSpeed;

        Vector2 velocity = playerRigid.linearVelocity;

        if (velocity.magnitude > PlayerInfo.Instance.maxSpeed)
        {
            playerRigid.linearVelocity = velocity.normalized * PlayerInfo.Instance.maxSpeed;
        }
    }
}