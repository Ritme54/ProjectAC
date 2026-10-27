using System;
using System.Collections.Generic;
using UnityEngine;

public class EliminationMode : MonoBehaviour
{
    // 적들의 체력 컴포넌트를 저장
    private List<MechHull2D> enemies = new List<MechHull2D>();

    private int remainingEnemies;
    private bool isCleared = false;

    // 섬멸전 클리어 이벤트
    public event Action OnCleared;

    private void Start()
    {
        FindEnemies();
    }

    private void FindEnemies()
    {
        enemies.Clear();

        // 현재 방에 배치된 Enemy 검색
        Enemy[] foundEnemies = GetComponentsInChildren<Enemy>();

        foreach (Enemy enemy in foundEnemies)
        {
            // 적의 체력 컴포넌트 가져오기
            MechHull2D hull = enemy.GetComponent<MechHull2D>();

            if (hull == null)
            {
                Debug.LogWarning(
                    $"[EliminationMode] {enemy.name}: MechHull2D가 없습니다."
                );
                continue;
            }

            // 이미 사망한 적은 제외
            if (hull.IsDead)
                continue;

            enemies.Add(hull);
        }

        remainingEnemies = enemies.Count;

        Debug.Log($"[EliminationMode] 발견한 적: {remainingEnemies}명");

        remainingEnemies = enemies.Count;
        initialEnemyCount = remainingEnemies;

    }

    private void LateUpdate()
    {
        if (isCleared)
            return;

        // 뒤에서부터 순회하며 사망한 적을 제거
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            MechHull2D hull = enemies[i];

            // 이미 파괴된 컴포넌트는 별도 처리하지 않음
            if (hull == null)
            {
                enemies.RemoveAt(i);
                remainingEnemies--;

                Debug.Log($"[EliminationMode] 파괴된 적 제거! 남은 적: {remainingEnemies}명");

                continue;
            }

            // 적의 체력이 0인지 확인
            if (hull.IsDead)
            {
                enemies.RemoveAt(i);
                remainingEnemies--;

                Debug.Log(
                    $"[EliminationMode] 적 사망! 남은 적: {remainingEnemies}명"
                );
            }
        }



        // 처음부터 적이 없었던 방은 자동 클리어하지 않음
        if (remainingEnemies == 0 && enemies.Count == 0)
        {
            // FindEnemies에서 적이 0명이었던 경우를 구분해야 함
            // 아래 초기 적 수 조건과 함께 검사
            if (initialEnemyCount > 0)
            {
                ClearRoom();
            }
        }
    }

    private int initialEnemyCount;

    private void ClearRoom()
    {
        if (isCleared)
            return;

        isCleared = true;

        Debug.Log("[EliminationMode] 섬멸전 클리어!");

        OnCleared?.Invoke();
    }
}