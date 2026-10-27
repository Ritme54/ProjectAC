using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPreFab;

    [SerializeField]
    private float spawnTime = 2f;
    private float timer = 0f;


    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnTime)
        {
            float randomX = Random.Range(-8f, 8f);
            float randomY = Random.Range(-4f, 4f);

            Instantiate(enemyPreFab, new Vector3(randomX, randomY, 0f), Quaternion.identity);
            timer = 0f;
        }
    }
}
