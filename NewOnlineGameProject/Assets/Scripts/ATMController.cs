using UnityEngine;
using Photon.Pun;

public class ATMController : MonoBehaviourPun
{
    [Header("ATM 设置")]
    public string requiredCardName = "Credit Card"; // 必须和 ItemData 里的 itemName 完全一致！
    public int minMoney = 1;      // 最少抽出多少钱
    public int maxMoney = 3000;   // 最多抽出多少钱 (如果目标是 2000，抽到 3000 就直接通关了)

    // 玩家按下 E 时调用
    public void TryUseATM(InventoryManager inventory)
    {
        // 1. 获取玩家当前【手里选中】的那一格物品
        ItemData selectedItem = inventory.GetSelectedItem();

        // 2. 检查他手里拿着的是不是指定的信用卡
        if (selectedItem != null && selectedItem.itemName == requiredCardName)
        {
            // 3. 随机抽取金额 (Random.Range 包含最小值，不包含最大值，所以 +1)
            int randomAmount = Random.Range(minMoney, maxMoney + 1);

            // 4. 刷完卡，没收/销毁这张卡
            inventory.RemoveSelectedItem();

            // 5. 呼叫撤离管家，直接把钱打进总进度！
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