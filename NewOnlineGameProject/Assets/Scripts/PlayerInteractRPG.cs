using UnityEngine;
using UnityEngine.UI; // 【新增】必须引入，为了控制 Image 组件
using TMPro;
using Photon.Pun;

public class PlayerInteractRPG : MonoBehaviourPun
{
    [Header("UI 设置")]
    public GameObject interactPanel;
    public TextMeshProUGUI interactPromptText;

    // 【新增】图标相关的 UI 引用
    public Image interactIcon;
    public Sprite defaultLootBoxIcon; // 盲盒的通用图标（可不填）

    private SciFiItemPickup currentItem;
    private LootBoxController currentLootBox;
    private InventoryManager inventory;

    void Start()
    {
        inventory = FindObjectOfType<InventoryManager>();
        if (interactPanel != null) interactPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (photonView != null && !photonView.IsMine) return;

        // 1. 检测是否靠近物品
        SciFiItemPickup item = other.GetComponent<SciFiItemPickup>();
        if (item != null)
        {
            currentItem = item;
            if (interactPromptText != null)
            {
                if (item.itemData != null)
                {
                    interactPromptText.text = $"[E] {item.itemData.itemName}\n<size=130><color=#FFD700>${item.itemData.value}</color></size>";

                    // 【新增】替换物品真实的图标并显示
                    if (interactIcon != null && item.itemData.icon != null)
                    {
                        interactIcon.sprite = item.itemData.icon;
                        interactIcon.gameObject.SetActive(true);
                    }
                }
                else
                {
                    interactPromptText.text = "[E] 未知物品";
                }
            }
            if (interactPanel != null) interactPanel.SetActive(true);
        }

        // 2. 检测是否靠近盲盒
        LootBoxController box = other.GetComponent<LootBoxController>();
        if (box != null)
        {
            currentLootBox = box;
            if (interactPromptText != null)
            {
                interactPromptText.text = "[E] Open";
            }

            // 【新增】盲盒的图标逻辑
            if (interactIcon != null)
            {
                if (defaultLootBoxIcon != null)
                {
                    // 如果你拖入了问号/箱子图标，就显示它
                    interactIcon.sprite = defaultLootBoxIcon;
                    interactIcon.gameObject.SetActive(true);
                }
                else
                {
                    // 如果没拖入任何通用图标，就直接隐藏图片节点，只留文字
                    interactIcon.gameObject.SetActive(false);
                }
            }

            if (interactPanel != null) interactPanel.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (photonView != null && !photonView.IsMine) return;

        if (other.GetComponent<SciFiItemPickup>() == currentItem)
        {
            currentItem = null;
            if (interactPanel != null) interactPanel.SetActive(false);
        }

        if (other.GetComponent<LootBoxController>() == currentLootBox)
        {
            currentLootBox = null;
            if (interactPanel != null) interactPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (photonView != null && !photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!PhotonNetwork.InRoom) return;

            if (currentItem != null)
            {
                currentItem.RequestPickup();
                currentItem = null;
                if (interactPanel != null) interactPanel.SetActive(false);
            }
            else if (currentLootBox != null)
            {
                currentLootBox.RequestOpen();
                currentLootBox = null;
                if (interactPanel != null) interactPanel.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) inventory.SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) inventory.SelectSlot(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) inventory.SelectSlot(5);

        if (Input.GetKeyDown(KeyCode.G))
        {
            DropCurrentItem();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            UseCurrentItem();
        }
    }

    private void UseCurrentItem()
    {
        if (inventory == null) return;

        // 获取当前选中的物品
        ItemData itemToUse = inventory.GetSelectedItem();

        // 如果格子里有东西，且类型是 Prop
        if (itemToUse != null && itemToUse.type == ItemType.Prop)
        {
            Debug.Log($"玩家使用了道具：{itemToUse.itemName}");

            // 1. 执行回血逻辑 (Debug 占位)
            if (itemToUse.healthRestore > 0)
            {
                // 未来你的血条系统做好了，就在这里呼叫它！
                Debug.Log($"<color=green>【系统提示】呲——！玩家恢复了 {itemToUse.healthRestore} 点生命值！</color>");
            }

            // 2. 执行加速逻辑 (呼叫我们刚才写的 WeightController)
            if (itemToUse.speedBoostMultiplier > 1f)
            {
                Debug.Log($"<color=yellow>【系统提示】扎针了！移速提升为 {itemToUse.speedBoostMultiplier} 倍，持续 {itemToUse.speedBoostDuration} 秒！</color>");

                PlayerWeightController weightController = GetComponent<PlayerWeightController>();
                if (weightController != null)
                {
                    weightController.ApplySpeedBuff(itemToUse.speedBoostMultiplier, itemToUse.speedBoostDuration);
                }
            }

            // 3. 消耗逻辑：如果是消耗品，用完直接从背包里清空！
            if (itemToUse.isConsumable)
            {
                inventory.RemoveSelectedItem();
                Debug.Log("道具已消耗，格子已清空。");
            }
        }
        else if (itemToUse != null && itemToUse.type == ItemType.Weapon)
        {
            Debug.Log("【系统提示】这是武器，你需要点击右键瞄准或执行开火逻辑，而不是把它吃掉！");
        }
    }

    private void DropCurrentItem()
    {
        if (inventory == null) return;
        ItemData itemToDrop = inventory.GetSelectedItem();
        if (itemToDrop != null)
        {
            Vector3 dropPosition = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;
            PhotonNetwork.Instantiate(itemToDrop.resourcePrefabName, dropPosition, Quaternion.identity);
            inventory.RemoveSelectedItem();
        }
    }
}