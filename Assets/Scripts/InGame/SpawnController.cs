using UnityEngine;
using UnityEngine.UIElements;

public class SpawnController : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;

    [SerializeField]
    private Transform centerTF;

    [SerializeField]
    private float spawnRadius = 3f;

    private void Start()
    {
        //Position offset = Random.insideUnitCircle

        Debug.Log(centerTF.position);


    }





}
