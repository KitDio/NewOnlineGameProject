using UnityEngine;
using Photon.Pun;

public class ATMController : MonoBehaviourPun
{
    [Header("ATM 设置")]
    public string requiredCardName = "Credit Card";
    public int minMoney = 1;
    public int maxMoney = 12000;  // 配合咱们刚才的四倍暴富数值

    [Header("抽奖概率设置")]
    [Tooltip("数值越大，抽到大奖的概率越低。1 为平均分布，3 为比较难，5 为极其稀有。")]
    [Range(1f, 10f)]
    public float rarityCurve = 4f;

    [Header("音效设置 (SFX)")]
    public AudioSource audioSource;       // 播放声音的组件（喇叭）
    public AudioClip successSound;        // 刷卡变现成功的多巴胺音效 (Cash Register 等)
    public AudioClip errorSound;          // 没拿卡时的错误提示音 (Buzzer/Error 等)

    // 玩家按下 E 时调用
    public void TryUseATM(InventoryManager inventory)
    {
        ItemData selectedItem = inventory.GetSelectedItem();

        if (selectedItem != null && selectedItem.itemName == requiredCardName)
        {
            // --- 【音效触发】播放刷卡成功的声音 ---
            if (audioSource != null && successSound != null)
            {
                audioSource.PlayOneShot(successSound);
            }

            // --- 【加权随机算法】 ---
            float roll = Random.value;
            float weightedRoll = Mathf.Pow(roll, rarityCurve);
            int randomAmount = Mathf.RoundToInt(Mathf.Lerp(minMoney, maxMoney, weightedRoll));

            inventory.RemoveSelectedItem();

            ExtractionManager extractionManager = FindObjectOfType<ExtractionManager>();
            if (extractionManager != null)
            {
                extractionManager.AddFundsDirectly(randomAmount);
            }
        }
        else
        {
            // --- 【音效触发】播放错误/拒绝的声音 ---
            if (audioSource != null && errorSound != null)
            {
                audioSource.PlayOneShot(errorSound);
            }
            Debug.LogWarning("ATM 提示：请将【信用卡】切到手中（按1~6），再按 E 刷卡！");
        }
    }
}