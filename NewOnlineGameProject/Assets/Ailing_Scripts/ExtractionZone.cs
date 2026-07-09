using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ExtractionZone : MonoBehaviour
{
    private ExtractionManager manager;

    void Start()
    {
        manager = FindObjectOfType<ExtractionManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInteractRPG player = other.GetComponentInParent<PlayerInteractRPG>();
        PhotonView pv = other.GetComponentInParent<PhotonView>();

        if (player != null && pv != null && pv.IsMine)
        {
            if (manager != null) manager.OpenExtractionUI();

            // 【新增】告诉全网：我进圈了！
            Hashtable hash = new Hashtable();
            hash.Add("InZone", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInteractRPG player = other.GetComponentInParent<PlayerInteractRPG>();
        PhotonView pv = other.GetComponentInParent<PhotonView>();

        if (player != null && pv != null && pv.IsMine)
        {
            if (manager != null) manager.CloseExtractionUI();

            // 【新增】告诉全网：我出圈了！
            Hashtable hash = new Hashtable();
            hash.Add("InZone", false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }
}