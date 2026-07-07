using UnityEngine;
using Photon.Pun;
using System.Collections; // 用于协程（倒计时延迟）

public class LootBoxController : MonoBehaviourPun
{
    [Header("盲盒组件")]
    public Rigidbody lidRigidbody; // 拖入盖子的 Rigidbody
    public Transform spawnPoint;   // 物品弹出的位置（可以在箱子里建一个空物体作为发射点）

    [Header("物理效果")]
    public float popForce = 5f;    // 盖子弹飞的力度

    [Header("奖池 (填入 Resources 文件夹里的预制体名字)")]
    public string[] possibleItems;

    private bool isOpened = false; // 防止重复开启

    // 玩家按下 E 时调用
    public void RequestOpen()
    {
        if (isOpened) return;
        photonView.RPC("RPC_TryOpen", RpcTarget.MasterClient);
    }

    [PunRPC]
    void RPC_TryOpen()
    {
        if (isOpened) return;
        isOpened = true; // 锁定箱子

        // 广播给所有人：箱子开了！
        photonView.RPC("RPC_ConfirmOpen", RpcTarget.All);
    }

    [PunRPC]
    void RPC_ConfirmOpen()
    {
        // 1. 所有人都在本地执行：弹飞盖子
        if (lidRigidbody != null)
        {
            lidRigidbody.isKinematic = false; // 解除物理锁定
            // 给盖子施加一个向上和向前的爆炸力，并加一点旋转，看起来更自然
            lidRigidbody.AddForce((Vector3.up + transform.forward) * popForce, ForceMode.Impulse);
            lidRigidbody.AddTorque(new Vector3(10f, 0, 10f), ForceMode.Impulse);
        }

        // 2. 只有主机负责在网络上生成物品和销毁箱子
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnItemAndDestroyBox());
        }
    }

    // 这是一个协程，可以让我们“等一会儿”再执行后续代码
    IEnumerator SpawnItemAndDestroyBox()
    {
        // 如果奖池里有东西，就随机抽一个生成
        if (possibleItems.Length > 0 && spawnPoint != null)
        {
            int randomIndex = Random.Range(0, possibleItems.Length);
            string itemToSpawn = possibleItems[randomIndex];

            // 在网络上生成这个物品，它自带之前写好的拾取脚本
            PhotonNetwork.Instantiate(itemToSpawn, spawnPoint.position, Quaternion.identity);
        }

        // 倒计时 1 秒
        yield return new WaitForSeconds(3f);

        // 销毁空箱子
        PhotonNetwork.Destroy(gameObject);
    }
}