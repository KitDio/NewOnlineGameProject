using UnityEngine;
using System.Collections;

public class Y_PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public Y_StatusOverlay statusOverlay;
    private bool isBurning = false;
    private bool isFrozen = false;

    private Rob01Ctrl playerController;

    void Start()
    {
        currentHealth = maxHealth;

        playerController = GetComponent<Rob01Ctrl>();
    }

    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Dead");
    }

    public void ApplyBurn()
    {
        if (isBurning)
            return;

        isBurning = true;

        Debug.Log("Player Burning");

        if (statusOverlay != null)
        {
            statusOverlay.ShowBurn(true);
        }

        StartCoroutine(BurnCoroutine());
    }

    public void ApplyFreeze()
    {
        if (isFrozen)
            return;

        isFrozen = true;

        if (statusOverlay != null)
        {
            statusOverlay.ShowFreeze(true);
        }

        playerController.SetMoveSpeed(3f);

        StartCoroutine(FreezeCoroutine());
    }

    IEnumerator BurnCoroutine()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1f);

            TakeDamage(2);

            Debug.Log("Burn Damage");
        }

        isBurning = false;

        if (statusOverlay != null)
        {
            statusOverlay.ShowBurn(false);
        }

        Debug.Log("Burn End");
    }

    IEnumerator FreezeCoroutine()
    {
        yield return new WaitForSeconds(3f);

        playerController.ResetMoveSpeed();

        if (statusOverlay != null)
        {
            statusOverlay.ShowFreeze(false);
        }

        isFrozen = false;

        Debug.Log("Freeze End");
    }
}