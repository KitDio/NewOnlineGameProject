using UnityEngine;
using Photon.Pun;

public class ExtractionZone : MonoBehaviour
{
    private ExtractionManager manager;

    void Start()
    {
        // 自动去场景里找大管家
        manager = FindObjectOfType<ExtractionManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 【关键修复】使用 GetComponentInParent，哪怕碰到了玩家的手脚模型，也能向上找到本体脚本
        PlayerInteractRPG player = other.GetComponentInParent<PlayerInteractRPG>();
        PhotonView pv = other.GetComponentInParent<PhotonView>();

        // 确保玩家存在、网络组件存在，且这个角色是我自己控制的
        if (player != null && pv != null && pv.IsMine)
        {
            if (manager != null) manager.OpenExtractionUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInteractRPG player = other.GetComponentInParent<PlayerInteractRPG>();
        PhotonView pv = other.GetComponentInParent<PhotonView>();

        if (player != null && pv != null && pv.IsMine)
        {
            if (manager != null) manager.CloseExtractionUI();
        }
    }
}