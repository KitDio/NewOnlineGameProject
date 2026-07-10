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

    [Header("“Ù–ß…Ë÷√ (SFX)")]
    public AudioSource audioSource;
    public AudioClip dropSound; 
    public AudioClip healSound; 
    public AudioClip speedBoostSound;
    public AudioClip pickupSound;

    void Start()
    {
        inventory = FindObjectOfType<InventoryManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (photonView != null && !photonView.IsMine) return;

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
                InteractUIManager.Instance.interactIcon.gameObject.SetActive(false);

            InteractUIManager.Instance.interactPanel.SetActive(true);
        }

        VendingMachineController vendingMachine = other.GetComponent<VendingMachineController>();
        if (vendingMachine != null)
        {
            currentVendingMachine = vendingMachine;
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
                if (inventory != null && inventory.IsFull())
                {
                    return;
                }

                if (audioSource != null && pickupSound != null)
                {
                    audioSource.PlayOneShot(pickupSound);
                }

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

        ItemData itemToUse = inventory.GetSelectedItem();

        if (itemToUse != null && itemToUse.type == ItemType.Prop)
        {

            NetworkHealth health = GetComponent<NetworkHealth>();
            if (health != null)
            {
                if (health.currentHealth > 0 && health.currentHealth < health.maxHealth && itemToUse.healthRestore > 0)
                {
                    health.ApplyHeal(itemToUse.healthRestore);

                    if (audioSource != null && healSound != null)
                    {
                        audioSource.PlayOneShot(healSound);
                    }
                }
            }

            if (itemToUse.speedBoostMultiplier > 1f)
            {
                PlayerWeightController weightController = GetComponent<PlayerWeightController>();
                if (weightController != null)
                {
                    weightController.ApplySpeedBuff(itemToUse.speedBoostMultiplier, itemToUse.speedBoostDuration);
                }

                if (audioSource != null && speedBoostSound != null)
                {
                    audioSource.PlayOneShot(speedBoostSound);
                }
            }

            if (itemToUse.isConsumable)
            {
                inventory.RemoveSelectedItem();
            }
        }
    }
}