using UnityEngine;

public class Y_EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;

    private int currentHealth;

    private bool isDead = false;

    private Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log(gameObject.name + " Ê£ÓàÑªÁ¿£º" + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        isDead = true;

        Debug.Log(gameObject.name + " ËÀÍö");

        anim.SetBool("die", true);
    }
}