using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class QuickTestConnect : MonoBehaviourPunCallbacks
{
    [Header("玩家生成设置")]
    public string playerPrefabName = "PlayerArmature"; // 填入你 Resources 里的玩家预制体名字
    public Transform spawnPoint; // 把场景里的一个空物体拖进来当出生点，不拖就在坐标 0,0,0 生成

    [Header("敌人生成")]
    public GameObject[] enemyPrefabs;
    public Transform[] enemySpawnPoints;
    void Start()
    {
        Debug.Log("1. 开始尝试连接 Photon 服务器...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("2. 成功连接到服务器！现在准备加入或创建 TestRoom...");
        PhotonNetwork.JoinOrCreateRoom("TestRoom", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("3. 成功进入房间！当前房间名称: " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("当前房间人数: " + PhotonNetwork.CurrentRoom.PlayerCount);

        // 【核心修复】在这里生成玩家！只有进房间了，实例化才有效！
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        PhotonNetwork.Instantiate(playerPrefabName, spawnPos, Quaternion.identity);

        Debug.Log("4. 玩家网络生成指令已发送！");

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < enemySpawnPoints.Length; i++)
            {
                PhotonNetwork.Instantiate(
                    enemyPrefabs[i].name,
                    enemySpawnPoints[i].position,
                    enemySpawnPoints[i].rotation);
            }
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("进入房间失败，原因: " + message);
    }
}