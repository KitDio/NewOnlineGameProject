using UnityEngine;
using Photon.Pun;

public class SciFiItemPickup : MonoBehaviourPun
{
    public ItemData itemData;
    private bool isBeingPickedUp = false;

    // 玩家按下 E 时触发
    public void RequestPickup()
    {
        if (isBeingPickedUp) return;
        photonView.RPC("RPC_TryPickup", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void RPC_TryPickup(int playerActorNumber)
    {
        if (isBeingPickedUp) return;
        isBeingPickedUp = true;
        photonView.RPC("RPC_ConfirmPickup", RpcTarget.All, playerActorNumber);
    }

    [PunRPC]
    void RPC_ConfirmPickup(int playerActorNumber)
    {
        // 如果是我抢到的，呼叫我的背包大管家把东西装进去
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerActorNumber)
        {
            InventoryManager localInventory = FindObjectOfType<InventoryManager>();
            if (localInventory != null)
            {
                localInventory.AddItem(itemData);
            }
        }

        // 主机负责销毁地上的模型
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}