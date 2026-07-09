using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Y_EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public bool IsDead { get; private set; } = false;

    private Animator anim;

    public float destroyDelay = 5f;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        currentHealth -= damage;

        Debug.Log(gameObject.name + " Ê£ÓàÑªÁ¿£º" + currentHealth);

        if (currentHealth > 0)
        {
            StartCoroutine(HitReaction());
        }

        anim.SetTrigger("hitLeft");
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        IsDead = true;

        StopAllCoroutines();

        anim.SetBool("hitLeft", false);
        anim.SetBool("die", true);

        Debug.Log(gameObject.name + " ËÀÍö");

        StartCoroutine(DestroyAfterDelay());
    }

    IEnumerator HitReaction()
    {
        anim.SetBool("hitLeft", true);

        yield return new WaitForSeconds(0.2f);

        anim.SetBool("hitLeft", false);
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}