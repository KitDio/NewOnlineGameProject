using Photon.Pun;
using UnityEngine;

public class Y_EnemySpawner : MonoBehaviourPunCallbacks
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    void Start()
    {
        Debug.Log("Master = " + PhotonNetwork.IsMasterClient);
        Debug.Log("InRoom = " + PhotonNetwork.InRoom);

        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("不是Master");
            return;
        }

        Debug.Log("我是Master，开始生成");

        SpawnEnemies();
    }

    void SpawnEnemies()
    {
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