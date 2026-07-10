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
    private VendingMachineController currentVendingMachine;

    [Header("音效设置 (SFX)")]
    public AudioSource audioSource;       // 玩家身上的喇叭
    public AudioClip dropSound;           // 丢弃物品的音效
    public AudioClip healSound;           // 回血的音效
    public AudioClip speedBoostSound;     // 加速的音效
    public AudioClip pickupSound;

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
            InteractUIManager.Instance.interactPromptText.text = "[E] Swipe Card";

            if (InteractUIManager.Instance.interactIcon != null)
                InteractUIManager.Instance.interactIcon.gameObject.SetActive(false); // ATM不需要显示图标

            InteractUIManager.Instance.interactPanel.SetActive(true);
        }

        VendingMachineController vendingMachine = other.GetComponent<VendingMachineController>();
        if (vendingMachine != null)
        {
            currentVendingMachine = vendingMachine;
            // 动态显示价格
            InteractUIManager.Instance.interactPromptText.text = $"[E] Buy Energy Drink\n<size=28><color=red>-${vendingMachine.price}</color></size>";

            if (InteractUIManager.Instance.interactIcon != null)
                InteractUIManager.Instance.interactIcon.gameObject.SetActive(false);

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

        if (other.GetComponent<VendingMachineController>() == currentVendingMachine)
        {
            currentVendingMachine = null;
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
                // 先检查自己能不能装得下！
                if (inventory != null && inventory.IsFull())
                {
                    Debug.LogWarning("背包已满，无法拾取该物品！");
                    return;
                }

                // --- 【音效触发】背包没满，播放拾取声音！ ---
                if (audioSource != null && pickupSound != null)
                {
                    audioSource.PlayOneShot(pickupSound);
                }

                // 然后再正常走网络拾取流程
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
            else if (currentATM != null)
            {
                currentATM.TryUseATM(inventory);
            }

            else if (currentVendingMachine != null) 
            {
                currentVendingMachine.TryBuyItem();
            }
        }

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
            // --- 【音效触发】播放丢弃物品声音 ---
            if (audioSource != null && dropSound != null)
            {
                audioSource.PlayOneShot(dropSound);
            }

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

            // 1. 执行回血逻辑 
            NetworkHealth health = GetComponent<NetworkHealth>();
            if (health != null)
            {
                // 只有没死、没满血、且该道具确实包含回血数值时，才执行
                if (health.currentHealth > 0 && health.currentHealth < health.maxHealth && itemToUse.healthRestore > 0)
                {
                    health.ApplyHeal(itemToUse.healthRestore);

                    // --- 【音效触发】在这里精准播放回血声音 ---
                    if (audioSource != null && healSound != null)
                    {
                        audioSource.PlayOneShot(healSound);
                    }
                }
            }

            // 2. 执行加速逻辑
            if (itemToUse.speedBoostMultiplier > 1f)
            {
                Debug.Log($"<color=yellow>【系统提示】扎针了！移速提升为 {itemToUse.speedBoostMultiplier} 倍，持续 {itemToUse.speedBoostDuration} 秒！</color>");

                PlayerWeightController weightController = GetComponent<PlayerWeightController>();
                if (weightController != null)
                {
                    weightController.ApplySpeedBuff(itemToUse.speedBoostMultiplier, itemToUse.speedBoostDuration);
                }

                // --- 【音效触发】在这里精准播放加速/喝饮料声音 ---
                if (audioSource != null && speedBoostSound != null)
                {
                    audioSource.PlayOneShot(speedBoostSound);
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