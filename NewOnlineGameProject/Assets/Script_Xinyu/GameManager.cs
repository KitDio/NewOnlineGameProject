using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    // 完美复用你同学写的变量
    [Header("玩家生成设置")]
    public string playerPrefabName = "PlayerArmature";
    public Transform[] spawnPoints;

    void Start()
    {
        // 游戏场景加载完毕后，立刻检查玩家是否在线且在房间里
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            SpawnPlayer();
        }
        else
        {
            Debug.LogError("未连接网络或不在房间内，无法生成玩家！请从大厅重新进入。");
        }
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        // 【核心修改 2】：判断数组里有没有放东西
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Photon 给每个进房间的玩家都会发一个唯一的排队号 (ActorNumber)，从 1 开始排 (1, 2, 3, 4...)
            // 我们用 (排队号 - 1) 就可以刚好对应数组的第 0, 1, 2, 3 个出生点！
            int spawnIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;

            spawnPos = spawnPoints[spawnIndex].position;
            spawnRot = spawnPoints[spawnIndex].rotation;
        }

        // 生成玩家，并应用对应出生点的位置和朝向
        PhotonNetwork.Instantiate(playerPrefabName, spawnPos, spawnRot);

        Debug.Log("玩家已成功在游戏场景中生成！分配到的出生点编号：" + PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}