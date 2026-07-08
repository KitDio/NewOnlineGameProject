using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerInteractRPG : MonoBehaviourPun
{
    private SciFiItemPickup currentItem;
    private LootBoxController currentLootBox;
    private InventoryManager inventory;

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
                InteractUIManager.Instance.interactPromptText.text = $"[E] {item.itemData.itemName}\n<size=130><color=#FFD700>${item.itemData.value}</color></size>";

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
        }

        // 下方的数字键切换和丢弃逻辑保持不变
        if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) inventory.SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) inventory.SelectSlot(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) inventory.SelectSlot(5);

        if (Input.GetKeyDown(KeyCode.G)) DropCurrentItem();
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