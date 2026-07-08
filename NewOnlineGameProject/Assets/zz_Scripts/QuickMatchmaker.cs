using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class QuickMatchmaker : MonoBehaviourPunCallbacks
{
    [Tooltip("填入你在 Resources 文件夹下的玩家预制体名称")]
    public string playerPrefabName = "YourPlayerPrefabName";

    void Start()
    {
        // 第一步：启动游戏时，自动使用本地配置连接 Photon 服务器
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        // 第二步：成功连接服务器后，尝试随机加入房间，如果没有房间就自动建一个
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        // 第三步：成功进入房间后，在这个坐标生成玩家模型
        Vector3 spawnPosition = new Vector3(0f, 2f, 0f);
        PhotonNetwork.Instantiate(playerPrefabName, spawnPosition, Quaternion.identity);
    }
}