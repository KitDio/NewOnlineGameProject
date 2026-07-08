using UnityEngine;
using System.Collections;

public class Y_PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public Y_StatusOverlay statusOverlay;
    private bool isBurning = false;

    void Start()
    {
        currentHealth = maxHealth;
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
}