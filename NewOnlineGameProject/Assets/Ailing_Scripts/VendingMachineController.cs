using UnityEngine;
using Photon.Pun;

public class VendingMachineController : MonoBehaviourPun
{
    [Header("售货机设置")]
    public string itemPrefabName = "Energy Drink"; // 【关键】必须是你 Resources 文件夹里饮料预制体的准确名字！
    public int price = 300; // 饮料卖多少钱
    public Transform spawnPoint; // 饮料喷出来的位置

    [Header("弹射力设置 (抛物线效果)")]
    public float forwardForce = 5f;
    public float upwardForce = 3f;

    [Header("音效设置 (SFX)")]
    public AudioSource audioSource;         // 播放声音的组件（机器里的喇叭）
    public AudioClip dispenseSound;         // 购买成功，饮料掉落的清脆机械声
    public AudioClip errorSound;            // 余额不足时的“滴滴”拒绝声

    // 玩家按下 E 时调用
    public void TryBuyItem()
    {
        ExtractionManager extractionManager = FindObjectOfType<ExtractionManager>();

        if (extractionManager != null)
        {
            if (extractionManager.TrySpendFunds(price))
            {
                // --- 【音效触发】只在本地播放购买成功的声音 ---
                if (audioSource != null && dispenseSound != null)
                {
                    audioSource.PlayOneShot(dispenseSound);
                }

                Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position + transform.forward * 1.5f;

                // 1. 生成物体，并用 spawnedItem 变量接住它
                GameObject spawnedItem = PhotonNetwork.InstantiateRoomObject(itemPrefabName, spawnPos, Quaternion.identity);

                // 2. 尝试获取它身上的物理刚体组件
                if (spawnedItem != null)
                {
                    Rigidbody rb = spawnedItem.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        // 3. 计算力的方向：机器的正前方 + 正上方
                        Vector3 force = (transform.forward * forwardForce) + (transform.up * upwardForce);

                        // 4. 施加一个瞬间的冲击力 (Impulse)
                        rb.AddForce(force, ForceMode.Impulse);
                    }
                    else
                    {
                        Debug.LogWarning("售货机提示：你的饮料预制体上没有 Rigidbody 组件，无法实现弹射效果！");
                    }
                }
            }
            else
            {
                // --- 【音效触发】余额不足，本地播放错误声 ---
                if (audioSource != null && errorSound != null)
                {
                    audioSource.PlayOneShot(errorSound);
                }
                Debug.Log("滴滴滴！余额不足！");
            }
        }
    }
}