using Photon.Pun;
using UnityEngine;

public class Y_EnemySpawner : MonoBehaviourPunCallbacks
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        Debug.Log("SpawnEnemies Called");

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            PhotonNetwork.Instantiate(
                enemyPrefabs[i].name,
                spawnPoints[i].position,
                spawnPoints[i].rotation
            );
        }
    }
}