using UnityEngine;
using Photon.Pun;

public class ATMController : MonoBehaviourPun
{
    [Header("ATM")]
    public string requiredCardName = "Credit Card";
    public int minMoney = 1;
    public int maxMoney = 12000;

    [Header("Lucky Draw Probability Settings")]
    [Tooltip("数值越大，抽到大奖的概率越低。1 为平均分布，3 为比较难，5 为极其稀有。")]
    [Range(1f, 10f)]
    public float rarityCurve = 4f;

    [Header("SFX")]
    public AudioSource audioSource;
    public AudioClip successSound; 
    public AudioClip errorSound;

    public void TryUseATM(InventoryManager inventory)
    {
        ItemData selectedItem = inventory.GetSelectedItem();

        if (selectedItem != null && selectedItem.itemName == requiredCardName)
        {
            if (audioSource != null && successSound != null)
            {
                audioSource.PlayOneShot(successSound);
            }

            float roll = Random.value;
            float weightedRoll = Mathf.Pow(roll, rarityCurve);
            int randomAmount = Mathf.RoundToInt(Mathf.Lerp(minMoney, maxMoney, weightedRoll));

            inventory.RemoveSelectedItem();

            ExtractionManager extractionManager = FindObjectOfType<ExtractionManager>();
            if (extractionManager != null)
            {
                extractionManager.AddFundsDirectly(randomAmount);
            }
        }
        else
        {
            if (audioSource != null && errorSound != null)
            {
                audioSource.PlayOneShot(errorSound);
            }
        }
    }
}