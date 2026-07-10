using UnityEngine;
using UnityEngine.UI;
using TMPro; // 引入 TextMeshPro 命名空间

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Data")]
    public ItemData[] inventorySlots = new ItemData[6];

    [Header("Iventory UI")]
    public Image[] slotUIIcons = new Image[6];
    public GameObject[] selectionHighlights = new GameObject[6];

    [Header("Information UI")]
    public TextMeshProUGUI totalValueText; 
    public TextMeshProUGUI totalWeightText;

    public int currentSelectedIndex = 0;

    void Start()
    {
        SelectSlot(0);
        UpdateStatsUI();
    }

    public bool AddItem(ItemData itemToAdd)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
            {
                inventorySlots[i] = itemToAdd;
                slotUIIcons[i].sprite = itemToAdd.icon;
                slotUIIcons[i].enabled = true;

                UpdateStatsUI();

                return true;
            }
        }
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

    public void RemoveSelectedItem()
    {
        inventorySlots[currentSelectedIndex] = null;
        slotUIIcons[currentSelectedIndex].sprite = null;
        slotUIIcons[currentSelectedIndex].enabled = false;
 
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

    public void UpdateStatsUI()
    {
        if (totalValueText != null)
        {
            totalValueText.text = $"Total Value: <color=#FFD700>${GetTotalValue()}</color>";
        }

        if (totalWeightText != null)
        {
            float currentWeight = GetTotalWeight();
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
        UpdateStatsUI();
    }

    public bool IsFull()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
            {
                return false;
            }
        }
        return true;
    }
}