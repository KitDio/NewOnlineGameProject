using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class QuickTestConnect : MonoBehaviourPunCallbacks
{
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

    // ========== 这里是你需要的判断逻辑 ==========
    public override void OnJoinedRoom()
    {
        // 当控制台打印出这句话时，说明玩家已经彻底进房间了！
        Debug.Log("3. 成功进入房间！当前房间名称: " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("当前房间人数: " + PhotonNetwork.CurrentRoom.PlayerCount);

        // 如果你需要在这里执行一些逻辑（比如生成玩家模型），就可以写在这里
    }

    // 额外附赠一个报错监控，如果进房间失败了，至少知道为什么
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("进入房间失败，原因: " + message);
    }
}