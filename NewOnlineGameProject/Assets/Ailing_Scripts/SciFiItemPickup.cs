using UnityEngine;
using Photon.Pun;

public class SciFiItemPickup : MonoBehaviourPun
{
    public ItemData itemData;
    private bool isBeingPickedUp = false;

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
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerActorNumber)
        {
            InventoryManager localInventory = FindObjectOfType<InventoryManager>();
            if (localInventory != null)
            {
                localInventory.AddItem(itemData);
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}