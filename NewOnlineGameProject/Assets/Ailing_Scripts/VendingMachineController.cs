using UnityEngine;
using Photon.Pun;

public class VendingMachineController : MonoBehaviourPun
{
    [Header("售货机设置")]
    public string itemPrefabName = "Energy Drink";
    public int price = 300; 
    public Transform spawnPoint;

    [Header("弹射力设置 (抛物线效果)")]
    public float forwardForce = 5f;
    public float upwardForce = 3f;

    [Header("音效设置 (SFX)")]
    public AudioSource audioSource;
    public AudioClip dispenseSound;
    public AudioClip errorSound;

    public void TryBuyItem()
    {
        ExtractionManager extractionManager = FindObjectOfType<ExtractionManager>();

        if (extractionManager != null)
        {
            if (extractionManager.TrySpendFunds(price))
            {
                if (audioSource != null && dispenseSound != null)
                {
                    audioSource.PlayOneShot(dispenseSound);
                }

                Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position + transform.forward * 1.5f;

                GameObject spawnedItem = PhotonNetwork.InstantiateRoomObject(itemPrefabName, spawnPos, Quaternion.identity);

                if (spawnedItem != null)
                {
                    Rigidbody rb = spawnedItem.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 force = (transform.forward * forwardForce) + (transform.up * upwardForce);

                        rb.AddForce(force, ForceMode.Impulse);
                    }
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
}