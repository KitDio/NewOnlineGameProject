using UnityEngine;

public enum ItemType
{
    Weapon,
    Prop, 
    Treasure 
}

[CreateAssetMenu(fileName = "NewItem", menuName = "SciFi_Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("basic information")]
    public string itemName = "item";
    public ItemType type;
    public Sprite icon;  
    public GameObject prefab; 

    [Header("item attribute")]
    public float weight = 1f;
    public int value = 0;  

    [Header("respawn object")]
    public string resourcePrefabName;

    [Header("special attribute")]
    public bool isConsumable = true; 
    public float healthRestore = 0f; 

    [Header("speedup")]
    public float speedBoostMultiplier = 1f;
    public float speedBoostDuration = 5f; 
}