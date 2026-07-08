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

    // 玩家按下 E 时调用
    public void TryUseATM(InventoryManager inventory)
    {
        ItemData selectedItem = inventory.GetSelectedItem();

        if (selectedItem != null && selectedItem.itemName == requiredCardName)
        {
            // --- 【核心修改：加权随机算法】 ---

            // 1. 先摇一个 0.0 到 1.0 之间的纯随机小数 (比如 0.5)
            float roll = Random.value;

            // 2. 利用乘方(Pow)把这个小数“压”下去。
            // 比如 0.5 的 4 次方是 0.0625。原本 50% 的位置被强行压到了 6% 的位置！
            float weightedRoll = Mathf.Pow(roll, rarityCurve);

            // 3. 把这个被压扁的比例，映射到我们的金额区间里
            int randomAmount = Mathf.RoundToInt(Mathf.Lerp(minMoney, maxMoney, weightedRoll));

            // ---------------------------------

            inventory.RemoveSelectedItem();

            ExtractionManager extractionManager = FindObjectOfType<ExtractionManager>();
            if (extractionManager != null)
            {
                extractionManager.AddFundsDirectly(randomAmount);
            }
        }
        else
        {
            Debug.LogWarning("ATM 提示：请将【信用卡】切到手中（按1~6），再按 E 刷卡！");
        }
    }
}