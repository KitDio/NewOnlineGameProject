using Photon.Pun;
using UnityEngine;

public class Y_EnemySpawner : MonoBehaviourPunCallbacks
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    void Start()
    {
        Debug.Log("Spawner Start");

        Debug.Log("IsMaster = " + PhotonNetwork.IsMasterClient);

        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Not Master");
            return;
        }

        Debug.Log("Before SpawnEnemies");

        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        Debug.Log($"SpawnPoints Count = {spawnPoints.Length}");

        foreach (Transform spawnPoint in spawnPoints)
        {
            Debug.Log("Spawn At : " + spawnPoint.name);

            GameObject prefab = enemyPrefabs[
                Random.Range(0, enemyPrefabs.Length)
            ];

            PhotonNetwork.Instantiate(
                prefab.name,
                spawnPoint.position,
                spawnPoint.rotation
            );
        }
    }
}