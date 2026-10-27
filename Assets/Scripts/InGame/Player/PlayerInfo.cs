using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public float curWeight;
    public float limitWeight;

    public float maxSpeed;
    public float axel;

    private float playerHp;
    public float PlayerHp
    {
        get
        {
            return playerHp;
        }
        set
        {
            playerHp = value;

            if (playerHp < 0f)
            {
                playerHp = 0f;
            }
        }
    }

    private static PlayerInfo instance;
    public static PlayerInfo Instance
    {
        get
        {
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
