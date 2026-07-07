using UnityEngine;
using UnityEngine.UI; // 必须引入 UI 命名空间才能控制 Image

public class InventoryManager : MonoBehaviour
{
    [Header("背包数据 (严格限制6格)")]
    // 这是一个长度为6的数组，里面存的是咱们上一步写的 ItemData
    public ItemData[] inventorySlots = new ItemData[6];

    [Header("UI 引用 (拖入那6个格子的Image)")]
    // 这个数组用来存放UI上那6个负责显示图标的 Image 控件
    public Image[] slotUIIcons = new Image[6];

    // 这个方法留给“拾取物品”的代码调用
    public bool AddItem(ItemData itemToAdd)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            // 遍历这6个格子，找到第一个空的（null）
            if (inventorySlots[i] == null)
            {
                // 1. 把物品数据存入后台数组
                inventorySlots[i] = itemToAdd;

                // 2. 更新前端 UI：把图标替换成物品的图标，并让它显示出来
                slotUIIcons[i].sprite = itemToAdd.icon;
                slotUIIcons[i].enabled = true;

                Debug.Log($"成功捡起: {itemToAdd.itemName}，放在了第 {i + 1} 格！");

                // TODO: 可以在这里顺便调用“重新计算总负重”的方法

                return true; // 拾取成功，返回 true
            }
        }

        // 如果循环走完都没找到空位，说明6个格子全满了
        Debug.LogWarning("背包已满，无法拾取！");
        return false; // 拾取失败，返回 false
    }
}