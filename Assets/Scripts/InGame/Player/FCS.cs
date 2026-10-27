using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FCS : MonoBehaviour
{
    //진행방향을 내가 입력한 무브먼트로부터 가져오고, 진행방향과 플레이어 - 적까지의 방향을 비교해서 같은 방향에 있는 애들 우선 탐색.
    private void Update()
    {
        Gizmos.DrawWireSphere(transform.position, 90f);
    }
}
