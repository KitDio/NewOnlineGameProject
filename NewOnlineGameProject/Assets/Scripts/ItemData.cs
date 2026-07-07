using UnityEngine;

// 定义物品的三大种类，方便后续代码判断
public enum ItemType
{
    Weapon,   // 武器
    Prop,     // 道具 (增益/减益)
    Treasure  // 宝物 (搜打撤核心)
}

[CreateAssetMenu(fileName = "NewItem", menuName = "SciFi_Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("基础信息")]
    public string itemName = "未命名物品";
    public ItemType type;
    public Sprite icon;       // 背包UI里显示的2D图标
    public GameObject prefab; // 丢在地上时的3D模型实体

    [Header("搜打撤核心属性")]
    public float weight = 1f; // 重量 (用于计算超重减速)
    public int value = 0;     // 价值 (撤离变卖用)

    [Header("联机生成设置")]
    public string resourcePrefabName;

    [Header("道具特殊属性 (Prop专属)")]
    public bool isConsumable = true;      // 是否是一次性消耗品 (默认打勾，吃完就没)
    public float healthRestore = 0f;      // 回血量 (如果是血包就填 50)

    [Header("加速 Buff 属性")]
    public float speedBoostMultiplier = 1f; // 移速倍率 (填 1 为不加速，填 1.5 为提速50%)
    public float speedBoostDuration = 5f;   // 加速持续时间 (秒)
}