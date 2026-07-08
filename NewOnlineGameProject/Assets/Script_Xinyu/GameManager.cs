using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("--- 玩家生成设置 ---")]
    [Tooltip("必须和 Resources 文件夹里的预制体名字一模一样")]
    public string playerPrefabName = "PlayerArmature";

    [Header("--- 出生点设置 ---")]
    [Tooltip("可以拖入场景里的几个空物体作为出生点，防止大家挤在一起")]
    public Transform[] spawnPoints;

    void Start()
    {
        // 防御机制：确保玩家是真的连着网进来的，而不是你直接在 Game 场景里点 Play 测试
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();
        }
        else
        {
            Debug.LogError("离线状态！请从大厅 (LobbyScene) 开始运行游戏！");
        }
    }

    private void SpawnPlayer()
    {
        // 1. 简单的随机出生点逻辑（如果没设置出生点，就在原点 0,0,0 生成）
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            spawnPosition = spawnPoints[randomIndex].position;
            spawnRotation = spawnPoints[randomIndex].rotation;
        }

        // 2. 正式生成玩家！
        // 注意：这里用的是 PhotonNetwork.Instantiate，它会自动在所有玩家的屏幕上同步生成这个模型
        GameObject myPlayer = PhotonNetwork.Instantiate(playerPrefabName, spawnPosition, spawnRotation);

        // 3. 打印自己的名字（验证大厅传过来的数据）
        Debug.Log("玩家生成完毕！本机玩家名字读取成功: " + PhotonNetwork.LocalPlayer.NickName);
    }

    // 玩家离开房间时的统一处理（比如掉线了）
    public override void OnLeftRoom()
    {
        // 退回大厅场景（把 0 换成你的 LobbyScene 在 Build Settings 里的序号或名字）
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}