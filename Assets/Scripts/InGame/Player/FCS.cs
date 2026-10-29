using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FCS : MonoBehaviour
{
    //진행방향을 내가 입력한 무브먼트로부터 가져오고, 진행방향과 플레이어 - 적까지의 방향을 비교해서 같은 방향에 있는 애들 우선 탐색.

    // 진행방향, 내 하체와 상체가 둘 다 진행방향으로 움직임. 감지범위, 감지범위 그리기, 감지된 적들, 감지된 적들중 가장 가까운 적.

    Vector2 curDir = Vector2.up;
    Rigidbody2D playerRigid;

    Transform upperBody;

    private void Start()
    {
        playerRigid = GetComponent<Rigidbody2D>();
    }


    private void UpdateCurDir()
    {
        curDir = playerRigid.linearVelocity;
    }

    private void MovetoUpperBody()
    {
        //upperBody.rotation = Quaternion.Slerp(transform.rotation,);
    }
}
