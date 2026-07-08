using UnityEngine;
using UnityEngine.UI;
using TMPro; // 引入 TextMeshPro 命名空间

public class InventoryManager : MonoBehaviour
{
    [Header("背包数据 (严格限制6格)")]
    public ItemData[] inventorySlots = new ItemData[6];

    [Header("格子 UI 引用")]
    public Image[] slotUIIcons = new Image[6];
    public GameObject[] selectionHighlights = new GameObject[6];

    [Header("统计数据 UI")]
    public TextMeshProUGUI totalValueText;  // 拖入你的 TotalValueText
    public TextMeshProUGUI totalWeightText; // 拖入你的 TotalWeightText

    public int currentSelectedIndex = 0;

    void Start()
    {
        SelectSlot(0);
        UpdateStatsUI(); // 游戏开局强制刷新一次，显示 0
    }

    // 拾取物品
    public bool AddItem(ItemData itemToAdd)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
            {
                inventorySlots[i] = itemToAdd;
                slotUIIcons[i].sprite = itemToAdd.icon;
                slotUIIcons[i].enabled = true;

                // 【关键】包里多东西了，马上刷新统计数据！
                UpdateStatsUI();

                return true;
            }
        }
        Debug.LogWarning("背包已满，无法拾取！");
        return false;
    }

    public void SelectSlot(int index)
    {
        currentSelectedIndex = index;
        for (int i = 0; i < selectionHighlights.Length; i++)
        {
            if (selectionHighlights[i] != null)
            {
                selectionHighlights[i].SetActive(i == index);
            }
        }
    }

    public ItemData GetSelectedItem()
    {
        return inventorySlots[currentSelectedIndex];
    }

    // 丢弃或消耗物品
    public void RemoveSelectedItem()
    {
        inventorySlots[currentSelectedIndex] = null;
        slotUIIcons[currentSelectedIndex].sprite = null;
        slotUIIcons[currentSelectedIndex].enabled = false;

        // 包里少东西了，马上刷新统计数据！
        UpdateStatsUI();
    }

    public float GetTotalWeight()
    {
        float totalWeight = 0f;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
            {
                totalWeight += inventorySlots[i].weight;
            }
        }
        return totalWeight;
    }

    // 计算背包里的总价值
    public int GetTotalValue()
    {
        int totalValue = 0;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
            {
                totalValue += inventorySlots[i].value;
            }
        }
        return totalValue;
    }

    // 统一刷新 UI 文本的方法
    public void UpdateStatsUI()
    {
        if (totalValueText != null)
        {
            // 使用富文本把金钱数字变成金色
            totalValueText.text = $"Total Value: <color=#FFD700>${GetTotalValue()}</color>";
        }

        if (totalWeightText != null)
        {
            float currentWeight = GetTotalWeight();
            // 加个小彩蛋：如果负重超过 15，文字变成红色警告
            string weightColor = currentWeight >= 15f ? "red" : "white";
            totalWeightText.text = $"Weight: <color={weightColor}>{currentWeight} kg</color>";
        }
    }

    public void ClearAllItems()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i] = null;
            slotUIIcons[i].sprite = null;
            slotUIIcons[i].enabled = false;
        }
        UpdateStatsUI(); // 刷新负重和资产显示为 0
        Debug.Log("背包已清空！");
    }

    public bool IsFull()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
            {
                return false; // 只要找到了一个空位，就不算满
            }
        }
        return true; // 循环完了都没找到空位，说明满了
    }
}