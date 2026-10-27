using UnityEngine;

public class InputController : MonoBehaviour
{
    Rigidbody2D playerRigid;
    Vector2 inputVec;

    float limitWeightMaxSpeed;

    float normalMaxSpeed;

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
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (inputVec != Vector2.zero)
        {
            Vector2 force = inputVec * PlayerInfo.Instance.axel;
            playerRigid.AddForce(force, ForceMode2D.Force);
        }

        LimitSpeed();
    }

    private void LimitSpeed()
    {
        Vector2 velocity = playerRigid.linearVelocity;

        if (velocity.magnitude > PlayerInfo.Instance.maxSpeed)
        {
            playerRigid.linearVelocity = velocity.normalized * PlayerInfo.Instance.maxSpeed;
        }

        PlayerInfo.Instance.maxSpeed = PlayerInfo.Instance.curWeight >= PlayerInfo.Instance.limitWeight ? limitWeightMaxSpeed : normalMaxSpeed;
    }
}