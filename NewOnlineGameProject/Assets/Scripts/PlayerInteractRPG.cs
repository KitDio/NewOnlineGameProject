using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerInteractRPG : MonoBehaviourPun
{
    private SciFiItemPickup currentItem;
    private LootBoxController currentLootBox;
    private InventoryManager inventory;
    private ATMController currentATM;

    void Start()
    {
        inventory = FindObjectOfType<InventoryManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (photonView != null && !photonView.IsMine) return;

        // 如果场景里没有 UI 管家，直接终止，防报错
        if (InteractUIManager.Instance == null) return;

        SciFiItemPickup item = other.GetComponent<SciFiItemPickup>();
        if (item != null)
        {
            currentItem = item;
            if (item.itemData != null)
            {
                InteractUIManager.Instance.interactPromptText.text = $"[E] {item.itemData.itemName}\n<size=28><color=#FFD700>${item.itemData.value}</color></size>";

                if (InteractUIManager.Instance.interactIcon != null && item.itemData.icon != null)
                {
                    InteractUIManager.Instance.interactIcon.sprite = item.itemData.icon;
                    InteractUIManager.Instance.interactIcon.gameObject.SetActive(true);
                }
            }
            InteractUIManager.Instance.interactPanel.SetActive(true);
        }

        LootBoxController box = other.GetComponent<LootBoxController>();
        if (box != null)
        {
            currentLootBox = box;
            InteractUIManager.Instance.interactPromptText.text = "[E] Open";

            if (InteractUIManager.Instance.interactIcon != null)
            {
                if (InteractUIManager.Instance.defaultLootBoxIcon != null)
                {
                    InteractUIManager.Instance.interactIcon.sprite = InteractUIManager.Instance.defaultLootBoxIcon;
                    InteractUIManager.Instance.interactIcon.gameObject.SetActive(true);
                }
                else
                {
                    InteractUIManager.Instance.interactIcon.gameObject.SetActive(false);
                }
            }
            InteractUIManager.Instance.interactPanel.SetActive(true);
        }

        ATMController atm = other.GetComponent<ATMController>();
        if (atm != null)
        {
            currentATM = atm;
            InteractUIManager.Instance.interactPromptText.text = "[E] Swipe a Credit Card";

            if (InteractUIManager.Instance.interactIcon != null)
                InteractUIManager.Instance.interactIcon.gameObject.SetActive(false); // ATM不需要显示图标

            InteractUIManager.Instance.interactPanel.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (photonView != null && !photonView.IsMine) return;
        if (InteractUIManager.Instance == null) return;

        if (other.GetComponent<SciFiItemPickup>() == currentItem)
        {
            currentItem = null;
            InteractUIManager.Instance.interactPanel.SetActive(false);
        }

        if (other.GetComponent<LootBoxController>() == currentLootBox)
        {
            currentLootBox = null;
            InteractUIManager.Instance.interactPanel.SetActive(false);
        }

        if (other.GetComponent<ATMController>() == currentATM)
        {
            currentATM = null;
            InteractUIManager.Instance.interactPanel.SetActive(false);
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
                if (InteractUIManager.Instance != null) InteractUIManager.Instance.interactPanel.SetActive(false);
            }
            else if (currentLootBox != null)
            {
                currentLootBox.RequestOpen();
                currentLootBox = null;
                if (InteractUIManager.Instance != null) InteractUIManager.Instance.interactPanel.SetActive(false);
            }
            else if (currentATM != null) // 【新增 ATM 触发逻辑】
            {
                currentATM.TryUseATM(inventory);
            }
        }

        // 下方的数字键切换和丢弃逻辑保持不变
        if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) inventory.SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) inventory.SelectSlot(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) inventory.SelectSlot(5);

        if (Input.GetKeyDown(KeyCode.G)) DropCurrentItem();

        // 【补回】按 F 键使用当前选中的物品
        if (Input.GetKeyDown(KeyCode.F))
        {
            UseCurrentItem();
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
    private void UseCurrentItem()
    {
        if (inventory == null) return;

        // 获取当前选中的物品
        ItemData itemToUse = inventory.GetSelectedItem();

        // 如果格子里有东西，且类型是 Prop (道具)
        if (itemToUse != null && itemToUse.type == ItemType.Prop)
        {
            Debug.Log($"玩家使用了道具：{itemToUse.itemName}");

            // 1. 执行回血逻辑 (Debug 占位)
            if (itemToUse.healthRestore > 0)
            {
                Debug.Log($"<color=green>【系统提示】呲——！玩家恢复了 {itemToUse.healthRestore} 点生命值！</color>");
            }

            // 2. 执行加速逻辑 (呼叫你身上的 WeightController)
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
        else if (itemToUse != null)
        {
            Debug.LogWarning($"【系统提示】这是 {itemToUse.type}，不能像消耗品一样直接使用！");
        }
    }
}