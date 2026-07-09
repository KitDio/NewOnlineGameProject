using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Y_EnemyHealth : MonoBehaviourPun, IPunObservable
{
    public int maxHealth = 100;
    public int currentHealth;

    public bool IsDead { get; private set; } = false;

    private Animator anim;

    public float destroyDelay = 5f;

    private bool playedDieAnimation = false;

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if (IsDead && !playedDieAnimation)
            {
                playedDieAnimation = true;

                anim.SetBool("hitLeft", false);
                anim.SetBool("die", true);
            }
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }
    [PunRPC]
    public void RPC_TakeDamage(int damage)
    {
        // 只有 Master 真正修改血量
        if (!PhotonNetwork.IsMasterClient)
            return;

        TakeDamage(damage);
    }
    public void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        currentHealth -= damage;

        Debug.Log(gameObject.name + " 剩余血量：" + currentHealth);

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

        Debug.Log(gameObject.name + " 死亡");

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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            Debug.Log("Health Writing: " + currentHealth);

            stream.SendNext(currentHealth);
            stream.SendNext(IsDead);
        }
        else
        {
            currentHealth = (int)stream.ReceiveNext();
            IsDead = (bool)stream.ReceiveNext();

            Debug.Log("Health Reading: " + currentHealth);
        }
    }
}